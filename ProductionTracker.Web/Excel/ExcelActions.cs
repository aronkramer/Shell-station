using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.IO;
using ProductionTracker.Data;
using System.Globalization;

namespace ProductionTracker.Web.Excel
{
    public static class ExcelActions
    {
        private static List<string> _erros = new List<string>();

        public static DataTable ConvertXSLXtoDataTable(HttpPostedFileBase file)
        {
            try
            {
                var stream = file.InputStream;
                var datatable = new DataTable();
                var workbook = new XLWorkbook(stream);
                var xlWorksheet = workbook.Worksheet(1);
                var range = xlWorksheet.Range(xlWorksheet.FirstCellUsed(), xlWorksheet.LastCellUsed());

                var col = range.ColumnCount();
                var row = range.RowCount();

                datatable.Clear();
                for (var i = 1; i <= col; i++)
                {
                    var column = xlWorksheet.Cell(1, i);

                    datatable.Columns.Add(column.Value.ToString());
                }

                var firstHeadRow = 0;
                foreach (var item in range.Rows())
                {
                    if (firstHeadRow != 0)
                    {
                        var array = new object[col];
                        for (var y = 1; y <= col; y++)
                        {
                            array[y - 1] = item.Cell(y).Value;
                        }

                        datatable.Rows.Add(array);
                    }
                    firstHeadRow++;
                }
                stream.Close();
                return datatable;
            }
            catch
            {
                return null;
            }
        }

        public static ErrorsAndItems ConvertProductoinToItems(ProductionForCT production)
        {
            var tempDate = production.Name.Split()[1];
            DateTime date;
            var isdate = DateTime.TryParseExact(tempDate, DateTimeFormats(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out date);
            if (!isdate)
            {
                date = DateTime.Now;
                AddErrorMsg($"Sorry the date {tempDate} is in the wrong format we are going to use the current date instead");
            }
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var items = new List<CuttingInstructionDetail>();
            foreach (var marker in production.Markers)
            {
                var markerCat = repo.GetMarkerCategory(marker.Name);
                if (NotNull(markerCat))
                {

                    var sizes = marker.Sizes;
                    foreach (var s in sizes)
                    {

                        foreach (var colmat in marker.ColorMaterials)
                        {
                            var colorId = repo.GetColorId(colmat.Color);
                            if (!NotNull(colorId))
                            {
                                colorId = repo.GetColorDetailsId(colmat.Color);
                            }
                            if (NotNull(colorId))
                            {
                                var mat = repo.GetMaterialId(colmat.Material);
                                if (NotNull(mat))
                                {
                                    var item = new Item
                                    {
                                        DepartmentId = markerCat.DepartmentId,
                                        BodyStyleId = markerCat.BodyStyleId,
                                        SleeveId = markerCat.SleeveId,
                                        ColorId = (int)colorId,
                                        MaterialId = (int)mat,
                                        SizeId = s.SizeId

                                    };
                                    var dbItem = repo.GetItem(item);
                                    if (NotNull(dbItem))
                                    {
                                        var itemQuantity = colmat.Layers * s.AmountPerLayer;
                                        if (itemQuantity > 0)
                                        {
                                            items.Add(new CuttingInstructionDetail
                                            {
                                                ItemId = dbItem.Id,
                                                Item = dbItem,
                                                Quantity = itemQuantity
                                            });
                                        }
                                    }
                                    else
                                    {
                                        AddErrorMsg("item", $"with size:{item.SizeId}, Material:{item.MaterialId},Color:{item.ColorId},Department:{item.DepartmentId},BodyStyle:{item.BodyStyleId}, Sleeve:{item.SleeveId}");
                                    }
                                }
                                else
                                {
                                    AddErrorMsg("materieal", colmat.Material);
                                }
                            }
                            else
                            {
                                AddErrorMsg("Color", colmat.Color);
                            }
                        }
                    }
                }
                else
                {
                    AddErrorMsg("marker", marker.Name);
                }
            }
            var returnItem = new ErrorsAndItems
            {
                Items = items,
                Errors = _erros,
                Date = date
            };
            _erros = new List<string>();
            return returnItem;
        }

        public static ProductionForCT ConvertCtToProduction(DataTable cuttingInstructions)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var production = new ProductionForCT
            {
                Name = cuttingInstructions.Columns[0].ColumnName,
            };
            for (int x = 0; x < cuttingInstructions.Rows.Count; x++)
            {
                if (String.IsNullOrEmpty((string)cuttingInstructions.Rows[x][0]))
                {
                    var temp = (string)cuttingInstructions.Rows[x][1];
                    temp = String.Concat(temp.Where(c => !char.IsWhiteSpace(c))).ToUpper();
                    if (!String.IsNullOrEmpty(temp))
                    {
                        var split = temp.Split('-');
                        string name = split[0];
                        List<SizeWithLayer> sizes = new List<SizeWithLayer>();
                        string size = "";
                        if (split.Count() == 2)
                        {
                            var tempSize = split[1];
                            var sizeId = repo.GetSizeId(tempSize);
                            if (NotNull(sizeId))
                            {
                                name = split[0];
                                sizes.Add(new SizeWithLayer { SizeId = (int)sizeId, AmountPerLayer = 6 });
                            }
                            else
                            {
                                AddErrorMsg("size", tempSize);
                            }
                        }
                        else if (split.Count() >= 2)
                        {
                            name = split[0];
                            var newMrker = split[1];
                            if (newMrker.Contains("NEWMARKER"))
                            {
                                sizes = NewMarkerSizeConcact(split);
                            }
                            else
                            {
                                AddErrorMsg($"Sorry [{String.Join("-", split)}] is the wrong format please change the format and try again!");
                            }
                        }

                        else
                        {
                            var marker = repo.GetMarkerCategory(split[0]);
                            var sizeFromMarker = new List<SizeWithLayer>();
                            if (marker != null)
                            {
                                sizeFromMarker = repo.GetMarkerDetails(marker.Id).Select(md =>
                                {
                                    return new SizeWithLayer { SizeId = md.SizeId, AmountPerLayer = md.AmountPerLayer };
                                }).ToList();
                            }
                            else
                            {
                                AddErrorMsg("marker", split[0]);
                            }
                            sizes = sizeFromMarker;
                        }
                        if (sizes.Count > 0)
                        {

                            var marker = new MarkerWithColorMaterials
                            {
                                Name = name,
                                Size = size,
                                Sizes = sizes
                            };
                            production.Markers.Add(marker);
                            for (int y = x + 1; y < cuttingInstructions.Rows.Count; y++)
                            {
                                x++;
                                var temp1 = (string)cuttingInstructions.Rows[x][0];
                                string color;
                                string matireal;
                                if (!String.IsNullOrEmpty(temp1))
                                {
                                    var list = SplitStringOnSpace(temp1);
                                    color = list.Count > 2 ? $"{list[0]} {list[1]}" : list[0];
                                    matireal = list[list.Count - 1];
                                    matireal = matireal == "NYLON" ? "LYCRA" : matireal;
                                }
                                else
                                {
                                    break;
                                }
                                var layers = (string)cuttingInstructions.Rows[x][1];
                                if (!String.IsNullOrEmpty(layers) && int.TryParse(layers, out int layersInt))
                                {
                                    marker.ColorMaterials.Add(new ColorMaterial
                                    {
                                        Color = color,
                                        Material = matireal,
                                        Layers = layersInt
                                    });
                                }
                                else { break; }
                            }
                        }
                    }
                }
            }
            return production;
        }

        private static List<SizeWithLayer> NewMarkerSizeConcact(string[] split)
        {

            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var marker = repo.GetMarkerCategory(split[0]);
            var sizeFromMarker = new List<SizeWithLayer>();
            if (marker != null)
            {
                sizeFromMarker = repo.GetMarkerDetails(marker.Id).Select(md =>
                {
                    return new SizeWithLayer { SizeId = md.SizeId };
                }).ToList();
            }
            else
            {
                AddErrorMsg("marker", split[0]);
            }
            for (int z = 2; z < split.Count(); z++)
            {
                if (split[1].Split('_').Count() > 1)
                {
                    AddErrorMsg($"Sorry [ {String.Join("-", split)} ] is the wrong format! {Environment.NewLine} Check if there is [-] bettween [new marker] and first size");
                    break;
                }
                if (int.TryParse(split[z], out int amountInt))
                {
                    if (split.Count() - 2 != sizeFromMarker.Count)
                    {
                        AddErrorMsg($"Sorry [ {String.Join("-", split)} ] does not contain all the sizes. There is [ {sizeFromMarker.Count} ] sizes for marker [ {split[0]} ]! {Environment.NewLine} Check if there is a [-] bettween [new marker] and first size");
                        break;
                    }
                    sizeFromMarker[z - 2].AmountPerLayer = amountInt;
                }
                else
                {
                    var tempSplit = split[z].Split('_');
                    if (tempSplit.Count() > 1)
                    {
                        sizeFromMarker.Clear();
                        var tempSize = tempSplit[0];
                        var tempAmount = tempSplit[1];
                        var sizeId = repo.GetSizeId(tempSize);
                        if (NotNull(sizeId) && int.TryParse(tempAmount, out int amountI))
                        {

                            sizeFromMarker.Add(new SizeWithLayer { SizeId = (int)sizeId, AmountPerLayer = amountI });
                        }
                        else
                        {
                            AddErrorMsg("size", tempSize);
                        }
                    }
                    else
                    {
                        AddErrorMsg($"Sorry [{split[z]}] is not a number! Please change the format!");
                        break;
                    }

                }
            }
            return sizeFromMarker;
        }

        private static List<string> SplitStringOnSpace(string text)
        {
            return text.Split().ToList();
        }

        private static bool NotNull(object thing)
        {
            return thing != null;
        }

        private static void AddErrorMsg(string msg)
        {
            _erros.Add($"{_erros.Count + 1}) {msg}");
        }

        private static void AddErrorMsg(string typeNotFound, string item)
        {
            _erros.Add($"{_erros.Count + 1}) The {typeNotFound} : [ {item} ] was not found");
        }

        private static string[] DateTimeFormats()
        {
            return new string[] {
                "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy", "MM/dd/yyyy",
                "M.d.yyyy", "M.dd.yyyy", "MM.d.yyyy","MM.dd.yyyy",
                "M-d-yyyy", "M-dd-yyyy", "MM-d-yyyy","MM-dd-yyyy",
            };
        }
    }
}
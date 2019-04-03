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
            var items = new List<CuttingInstructionItem>();
            foreach (var marker in production.Markers)
            {
                var markerCat = repo.GetMarkerCategory(marker.Name);
                if (markerCat.NotNull())
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
                                            items.Add(new CuttingInstructionItem
                                            {
                                                ItemId = dbItem.Id,
                                                Item = dbItem,
                                                Quantity = itemQuantity
                                            });
                                        }
                                    }
                                    else
                                    {
                                        var size = repo.GetSize(item.SizeId).Name;
                                        var material = repo.GetMaterial(item.MaterialId).Name;
                                        var color = repo.GetColor(item.ColorId).Name;
                                        var department = repo.GetDepartment(item.DepartmentId).Name;
                                        var bodyStyle = repo.GetBodyStyle(item.BodyStyleId).Name;
                                        var sleeve = repo.GetSleeve(item.SleeveId).Name;
                                        AddErrorMsg("item", $"with size:{size}, Material:{material},Color:{color},Department:{department},BodyStyle:{bodyStyle}, Sleeve:{sleeve}");
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
            _erros = new List<string>();
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var production = new ProductionForCT
            {
                Name = cuttingInstructions.Columns[0].ColumnName,
            };
            var tempDate = production.Name.Split()[1];
            DateTime date;
            var isdate = DateTime.TryParseExact(tempDate, DateTimeFormats(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out date);
            if (!isdate)
            {
                date = DateTime.Now;
                AddErrorMsg($"Sorry the date {tempDate} is in the wrong format we are going to use the current date instead");
            }
            production.Date = date;
            for (int x = 0; x < cuttingInstructions.Rows.Count; x++)
            {
                if (String.IsNullOrEmpty((string)cuttingInstructions.Rows[x][0]))
                {
                    var allSizes = false;
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
                            var sizeIs = repo.GetSize(tempSize);
                            if (NotNull(sizeIs))
                            {
                                name = split[0];
                                sizes.Add(new SizeWithLayer { SizeId = sizeIs.Id,Name = sizeIs.Name, AmountPerLayer = 6 });
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
                                sizeFromMarker = repo.GetDefaltMarkerDetails(marker.Id).Select(md =>
                                {
                                    return new SizeWithLayer { SizeId = md.SizeId, AmountPerLayer = md.AmountPerLayer, Name = md.Size.Name };
                                }).ToList();
                                allSizes = true;
                            }
                            else
                            {
                                AddErrorMsg("marker", split[0]);
                            }
                            sizes = sizeFromMarker;
                        }
                        if (true)
                        {

                            var marker = new MarkerWithColorMaterials
                            {
                                Name = name,
                                Size = size,
                                Sizes = sizes,
                                AllSizes = allSizes
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
                                    matireal = matireal == "LYCRA" ? "NYLON" : matireal;
                                }
                                else
                                {
                                    break;
                                }
                                var layers = (string)cuttingInstructions.Rows[x][1];
                                var packagingText = (string)cuttingInstructions.Rows[x][3];
                                var packaging = Packaging.BOX;
                                if(packagingText != null)
                                {
                                    if(packagingText.ToUpper() == "HANG")
                                    {
                                        packaging = Packaging.HANG;
                                    }
                                }
                                if (!String.IsNullOrEmpty(layers) && int.TryParse(layers, out int layersInt))
                                {
                                    marker.ColorMaterials.Add(new ColorMaterial
                                    {
                                        Color = color,
                                        Material = matireal,
                                        Layers = layersInt,
                                        Packaging = packaging
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

        public static List<string> GetErrors()
        {
            return _erros;
        }

        public static FinalProduction ConvertProductoinToCTs(ProductionForCT production)
        {
            _erros = new List<string>();
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var finalProduction = new FinalProduction();
            finalProduction.Date = production.Date;
            foreach (var marker in production.Markers)
            {

                var details = new List<CuttingItsructionDetailsWithItems>();
                var markerCat = repo.GetMarkerCategory(marker.Name);
                if (NotNull(markerCat))
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
                            colmat.ColorId = (int)colorId;
                            var mat = repo.GetMaterialId(colmat.Material);
                            if (NotNull(mat))
                            {
                                colmat.MaterialId = (int)mat;
                                var detail = new CuttingItsructionDetailsWithItems
                                {
                                    ColorMaterial = colmat,
                                    Items = new List<CuttingInstructionItem>()
                                };

                                var sizes = marker.Sizes;
                                foreach (var s in sizes)
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
                                    if(item.MaterialId == 3 && item.ColorId == 34)
                                    {
                                        item.ColorId = 36;
                                    }
                                    var dbItem = repo.GetItem(item);
                                    if (NotNull(dbItem))
                                    {
                                        var itemQuantity = colmat.Layers * s.AmountPerLayer;
                                        if (itemQuantity > 0)
                                        {
                                            detail.Items.Add(new CuttingInstructionItem
                                            {
                                                ItemId = dbItem.Id,
                                                Item = dbItem,
                                                Quantity = itemQuantity,
                                                Packaging = colmat.Packaging
                                            });

                                        }
                                    }
                                    else
                                    {
                                        var size = repo.GetSize(item.SizeId).Name;
                                        var material = repo.GetMaterial(item.MaterialId).Name;
                                        var color = repo.GetColor(item.ColorId).Name;
                                        var department = repo.GetDepartment(item.DepartmentId).Name;
                                        var bodyStyle = repo.GetBodyStyle(item.BodyStyleId).Name;
                                        var sleeve = repo.GetSleeve(item.SleeveId).Name;
                                        AddErrorMsg("item", $"with size:{size}, Material:{material},Color:{color},Department:{department},BodyStyle:{bodyStyle}, Sleeve:{sleeve}");
                                    }
                                }
                                details.Add(detail);
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
                    finalProduction.CuttingInstructions.Add(new CuttingInstructionWithMarker
                    {
                        LotNumber = marker.LotNumber,
                        Details = details,
                        Marker = new Finalmarker
                        {
                            MarkerCatId = markerCat.Id,
                            Name = markerCat.Name,
                            AllSizes = marker.AllSizes,
                            Sizes = marker.Sizes,
                            PlannedProductionId = marker.PlannedProductionId
                            
                        }
                    }
                );
                }
                else
                {
                    AddErrorMsg("marker", marker.Name);
                }

            }

            return finalProduction;
        }

        public static ProductionForCT ProductionToFormatForExcel(Production production)
        {
            var finalProd = new ProductionForCT();
            var cprer = new ItemMatColComparer();
            finalProd.Date = production.Date;
            finalProd.Name = $"Murex {production.Date.ToShortDateString()}";
            finalProd.Markers = production.CuttingInstructions.Select(ci =>
            {
                return new MarkerWithColorMaterials
                {
                    Name = ci.MarkerText,
                    LotNumber = ci.LotNumber,
                    ColorMaterials = ci.CuttingInstructionDetails.Select(c =>
                    {

                        //var amountPerLayer = ci.CuttingInstructionSizes.Count() > 0 ? ci.CuttingInstructionSizes.FirstOrDefault(s => s.SizeId == c.Item.SizeId).AmountPerLayer : ci.MarkerCategory.MarkerDetails.FirstOrDefault(s => s.SizeId == c.Item.SizeId).AmountPerLayer;
                        return new ColorMaterial
                        {
                            Color = c.Fabric.ColorDescriptionName ?? c.Fabric.Color.Name,
                            Material = c.Fabric.Material.Name,
                            Layers = c.Layers,
                            Packaging = c.Packaging
                        };

                    }).ToList()

                };
            }).ToList();
            return finalProd;
        }

        public static XLWorkbook CuttingInstruction (ProductionForCT productoinInRightFormat)
        {
            if(productoinInRightFormat != null)
            {
                var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Instruction");
                ws.Cell(1, 1).Value = productoinInRightFormat.Name;
                ws.Cell(1, 1).AddToNamed("Titles");
                var rowCount = 4;
                foreach (var marker in productoinInRightFormat.Markers)
                {
                    ws.Cell(rowCount,2).Value = marker.Name;
                    ws.Cell(rowCount,2).AddToNamed("Titles");
                    ws.Cell(rowCount,5).Value = $"Lot:{marker.LotNumber}";
                    ws.Cell(rowCount,5).AddToNamed("Titles");
                    foreach (var colMat in marker.ColorMaterials)
                    {
                        rowCount++;
                        ws.Cell(rowCount,1).Value = $"{colMat.Color} {colMat.Material}";
                        ws.Cell(rowCount,2).Value = colMat.Layers;
                        ws.Cell(rowCount,4).Value = colMat.Packaging.ToString();
                    }
                    rowCount++;
                    rowCount++;

                }
                var titlesStyle = wb.Style;
                titlesStyle.Font.Bold = true;
                titlesStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.NamedRanges.NamedRange("Titles").Ranges.Style = titlesStyle;
                ws.Column(1).AdjustToContents();
                return wb;
            }
            return null;
        }

        private static List<SizeWithLayer> NewMarkerSizeConcact(string[] split)
        {

            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var marker = repo.GetMarkerCategory(split[0]);
            var sizeFromMarker = new List<SizeWithLayer>();
            if (marker != null)
            {
                if (split.Count() > 2 && int.TryParse(split[2], out int amountInt))
                {
                    sizeFromMarker = repo.GetDefaltMarkerDetails(marker.Id).Select(md =>
                    {
                        return new SizeWithLayer { SizeId = md.SizeId,Name = md.Size.Name };
                    }).ToList();
                }
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
                        var tempSize = tempSplit[0];
                        var tempAmount = tempSplit[1];
                        var SizeIs = repo.GetSize(tempSize);
                        if (NotNull(SizeIs) && int.TryParse(tempAmount, out int amountI))
                        {

                            sizeFromMarker.Add(new SizeWithLayer { SizeId = SizeIs.Id, Name = SizeIs.Name, AmountPerLayer = amountI });
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
            return sizeFromMarker.Where(s => s.AmountPerLayer != 0).ToList();
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionTracker.Web.Excel;
using ProductionTracker.Data;
using System.Text;
using ClosedXML.Excel;
using System.IO;

namespace ProductionTracker.Web.Controllers
{
    public class ProductionController : Controller
    {
        public ActionResult NewProduction()
        {
            ViewBag.Message = TempData["Message"] != null ? TempData["Message"] : null;
            var items = Session["ItemsWithErrors"] != null ? (ErrorsAndItems)Session["ItemsWithErrors"] : null;
            Session["ItemsWithErrors"] = null;
            //items = items != null ? items : null;
            return View(items);
        }
        [HttpPost]
        public ActionResult NewProduction(HttpPostedFileBase cuttingTicket)
        {
            Session["file"] = cuttingTicket;
            var dT = ExcelActions.ConvertXSLXtoDataTable(cuttingTicket);
            var production = ExcelActions.ConvertCtToProduction(dT);
            production = AddLotNumbers(production);
            var errors = ExcelActions.GetErrors();
            Session["Production"] = production;
            return Json(new { production, errors },JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ConvertCTToItems(ProductionForCT production)
        {
            var prodItems = ExcelActions.ConvertProductoinToCTs(production);
            Session["prodItems"] = prodItems;
            prodItems.CuttingInstructions.ForEach(ct => GetMarkerText(ct.Marker));
            var errors = ExcelActions.GetErrors();
            return Json(new { prodItems = new {prodItems.Date, CuttingInstructions = prodItems.CuttingInstructions.Select(ct =>
            {
                return new
                {
                    ct.Marker,                   
                    ct.LotNumber,
                    Items = ct.Items.Select(i =>
                    {
                        return new
                        {
                            i.Id,
                            i.ItemId,
                            i.Quantity,
                            Item = new
                            {
                                i.Item.SKU
                            }
                        };
                    })
                };
            })
            }, errors }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public void SubmitProduction(FinalProduction production)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var prod = new Production { Date = production.Date };
            repo.AddProduction(prod);
            foreach (var cI in production.CuttingInstructions)
            {
                var cutInst = new CuttingInstruction
                {
                    ProductionId = prod.Id,
                    LotNumber = cI.LotNumber,
                    MarkerText = cI.Marker.MarkerSizeText,
                    MarkerCatId = cI.Marker.Id
                };
                repo.AddCuttingTicket(cutInst);
                if (!cI.Marker.AllSizes)
                {
                    var sizes = cI.Marker.Sizes.Select(s =>
                    {
                        return new CuttingInstructionSize
                        {
                            SizeId = s.SizeId,
                            AmountPerLayer = s.AmountPerLayer,
                            CuttingInstructId = cutInst.Id
                        };
                    });
                    repo.AddCTSizes(sizes);
                }
                
                var ctd = cI.Items.Select(cd => 
                {
                    return new CuttingInstructionDetail
                    {
                        CuttingInstructionId = cutInst.Id,
                        ItemId = cd.ItemId,
                        Quantity = cd.Quantity
                    };
                });
                repo.AddCTDetails(ctd);
            }
        }
        public ActionResult NewProductionConfimation()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SubmitCT(CuttingInstruction instruction,IEnumerable<CuttingInstructionDetail> items)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            repo.AddCuttingTicket(instruction);
            items = items.Select(i => { i.CuttingInstructionId = instruction.Id; return i; });
            repo.AddCTDetails(items);
            //TempData["Message"] = $"Sussessfully added a new cutting ticket: Id - {instruction.Id}, From date: {instruction.Date.ToShortDateString()} Lot# : {instruction.Lot_ ?? 0} => Number of items: {items.Count()}, Total items: {items.Sum(i => i.Quantity)}";
            Session["ItemsWithErrors"] = null;
            return RedirectToAction("NewProduction");
        }
        public ActionResult GetItemId(string sku)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            var item = repo.GetItem(sku);

            return item != null ? Json(new { item.Id, item.SKU }, JsonRequestBehavior.AllowGet): null;
        }
        public ActionResult GetProductionInProgress()
        {
            var hi = (ProductionForCT)Session["Production"];
            return Json((ProductionForCT)Session["Production"] ?? null, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetMarker(string markerName)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var marker = repo.GetMarkerCategory(markerName.ToUpper());
            return Json(new { marker = marker == null ? null : new { marker.Id, marker.Name } } , JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetValidatoinLists()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var mats = repo.GetMaterials();
            var sizes = repo.GetSizes();
            var markers = repo.GetMarkerCatergorys();
            var colors = repo.GetColors().ToList();
            colors.AddRange(repo.GetColorDetails().Select(c => { return new Color { Id = c.ColorId, Name = c.Name }; }));
            return Json(new
            {
                material = mats.Select(r => r.Name),
                colors = colors.Select(r => r.Name),
                sizes = sizes.Select(r => r.Name ),
                markers = markers.Select(r => r.Name),

            }, JsonRequestBehavior.AllowGet);
        }


        private ProductionForCT AddLotNumbers (ProductionForCT production)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            production.LastLotNumber = repo.LastLotNumber();
            production.Markers = production.Markers.Select((m, i) => {m.LotNumber = production.LastLotNumber + 1 + i; return m; }).ToList();
            return production;
            
        }
        private void GetMarkerText(Finalmarker marker)
        {
            var markerSize = new StringBuilder();
            markerSize.Append(marker.Name);
            if (!marker.AllSizes)
            {
                if (marker.Sizes.Count() == 1)
                {
                    markerSize.Append($"-{marker.Sizes.ToList()[0].Name}");
                }
                else if (marker.Sizes.Count() > 1)
                {
                    markerSize.Append("-NEWMARKER");
                    foreach (var size in marker.Sizes)
                    {
                        markerSize.Append($"-{size.Name}_{size.AmountPerLayer}");
                    }
                }
            }
            marker.MarkerSizeText = markerSize.ToString();
            //return markerSize.ToString();
            
        }
        public void DownloadCuttingInstuctions(int productionId)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var prod = repo.GetProductionForExcel(productionId);
            var prodexcel = ExcelActions.ProductionToFormatForExcel(prod);
            var sheet = ExcelActions.CuttingInstruction(prodexcel);
            var fileName = prod.Date.ToString("MM.dd");
            FileDownload(sheet, fileName);
        }

        public void FileDownload(XLWorkbook workbook, string fileName)
        {
            // Prepare the response
            HttpResponseBase httpResponse = Response;
            
            httpResponse.Clear();
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            httpResponse.AddHeader("content-disposition", $"attachment;filename=\"{fileName}.xlsx\"");

            // Flush the workbook to the Response.OutputStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                memoryStream.WriteTo(httpResponse.OutputStream);
                memoryStream.Close();
            }

            httpResponse.End();
        }
    }
}
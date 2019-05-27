using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionTracker.Web.Excel;
using ProductionTracker.Data;
using ProductionTracker.Data.Models;
using System.Text;
using ClosedXML.Excel;
using System.IO;
using ProductionTracker.Web.Models;
using Newtonsoft.Json;

namespace ProductionTracker.Web.Controllers
{
    public class ProductionController : Controller
    {
        //NEW PRODUCTION
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
            if(production.LastLotNumber < LotNumberIndex())
            {
                AddLotNumbers(production);
            }
            var prodItems = ExcelActions.ConvertProductoinToCTs(production);
            if(prodItems.CuttingInstructions.Count() > 0)
            { 
                Session["prodItems"] = prodItems;
                prodItems.CuttingInstructions.ForEach(ct => GetMarkerText(ct.Marker));
                var errors = ExcelActions.GetErrors();
                return Json(new { prodItems = new {prodItems.Date, CuttingInstructions = prodItems.CuttingInstructions.Select(ct =>
                {
                    return new
                    {
                        ct.Marker,                   
                        ct.LotNumber,
                        Details = ct.Details.Select(d=>
                        {
                            return new
                            {
                                d.ColorMaterial,
                                Items = d.Items.Select(i =>
                                {
                                    return new
                                    {
                                        i.Id,
                                        i.ItemId,
                                        i.Quantity,
                                        Item = new
                                        {
                                            i.Item.SKU
                                        },
                                        i.Packaging
                                    };
                                })
                            };
                        }),
                        
                    };
                })
                }, errors }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var errors = ExcelActions.GetErrors();
                return Json(new { prodItems = 0 ,errors}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public void SubmitProduction(FinalProduction production)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var prod = new Production { Date = production.Date };
            repo.AddProduction(prod);
            if(production.CuttingInstructions.Count() > 0)
            {
                var lastLot = LotNumberIndex();
                //if (lastLot >= production.CuttingInstructions[0].LotNumber)
                //{
                //    production.CuttingInstructions = production.CuttingInstructions.Select((m, i) => { m.LotNumber = lastLot + 1 + i; return m; }).ToList();
                //}
                if (lastLot != production.CuttingInstructions[0].LotNumber)
                {
                    production.CuttingInstructions = production.CuttingInstructions.Select((m, i) => { m.LotNumber = lastLot + i; return m; }).ToList();
                }
                repo.IncrementLotNumberCounter(production.CuttingInstructions.Count());
            }
            foreach (var cI in production.CuttingInstructions)
            {
                
                
                  var cutInst = new CuttingInstruction
                  {
                  ProductionId = prod.Id,
                  LotNumber = cI.LotNumber,
                  MarkerText = cI.Marker.MarkerSizeText,
                  MarkerId = MarkerId(cI.Marker),
                  PlannedProductionId = cI.Marker.PlannedProductionId
                  };
                repo.AddCuttingTicket(cutInst);
                //if (!cI.Marker.AllSizes)
                //{
                //    var sizes = cI.Marker.Sizes.Select(s =>
                //    {
                //        return new CuttingInstructionSize
                //        {
                //            SizeId = s.SizeId,
                //            AmountPerLayer = s.AmountPerLayer,
                //            CuttingInstructId = cutInst.Id
                //        };
                //    });
                //    repo.AddCTSizes(sizes);
                //}
                foreach (var ctd in cI.Details)
                {
                    var newCtd = new CuttingInstructionDetail
                    {
                        FabricId = GetFabricId(ctd.ColorMaterial.MaterialId, ctd.ColorMaterial.ColorId),
                        Layers = ctd.ColorMaterial.Layers,
                        CuttingInstructionId = cutInst.Id
                    };
                    repo.AddCTDetail(newCtd);
                    var ctdI = ctd.Items.Select(cd =>
                    {
                        return new CuttingInstructionItem
                        {
                            CuttingInstructionDetailsId = newCtd.Id,
                            ItemId = cd.ItemId,
                            Quantity = cd.Quantity,
                            Packaging = cd.Packaging
                        };
                    });
                    repo.AddCTItems(ctdI);
                }

                //var ctd = cI.Items.Select(cd => 
                //{
                //    return new CuttingInstructionItem
                //    {
                //        CuttingInstructionDetailsId = cutInst.Id,
                //        ItemId = cd.ItemId,
                //        Quantity = cd.Quantity,
                //        Packaging = cd.Packaging
                //    };
                //});
                //repo.AddCTDetails(ctd);
            }
            TempData["Message"] = $"You susseffully added a new production with {production.CuttingInstructions.Count()} cuttting instuctoins from lot number {production.CuttingInstructions[0].LotNumber} - {production.CuttingInstructions[production.CuttingInstructions.Count() - 1].LotNumber}." +
                $"<br/> {production.CuttingInstructions.Select(c => c.Details.Sum(co => co.Items.Count())).Sum()} Items. Total pieces: {production.CuttingInstructions.Select(c => c.Details.Sum(co => co.Items.Sum(i => i.Quantity))).Sum()} ";
        }

        public ActionResult GetLastLotNUmber()
        {
            return Json(LotNumberIndex(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDefaltSizesForAMarkerCat(string markerCatergoryName)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var sizes = repo.GetDefaltMarkerDetails(markerCatergoryName);
            return Json(sizes.OrderBy(a => a.SizeId).Select(c =>
            {
                return new
                {
                    c.SizeId,
                    c.Size.Name,
                    c.AmountPerLayer
                };
            }
            ), JsonRequestBehavior.AllowGet);

        }

        public ActionResult NewProductionConfimation()
        {
            return View();
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
        



        //DATA LISTS

        public ActionResult GetSKUsList()
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            var skus = repo.GetItems().Select(i =>
            {
                return new
                {
                    i.Id,
                    i.SKU
                };
            });
            return Json(skus, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProductionCats()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var prodCats = repo.GetProductionCatergories()
                .Select(pc =>
                {
                    return new
                    {
                        pc.Id,
                        pc.Name
                    };
                });
            return Json(prodCats, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetValidatoinLists()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var mats = repo.GetMaterials();
            var sizes = repo.GetSizes();
            var markers = repo.GetMarkerCatergorys();
            var colors = repo.GetColors().ToList();
            var plannedProductions = repo.GetPlannedProductions().Select(pp =>
            {
                return new
                {
                    pp.Id,
                    Name = $"{pp.ProductionCatergory.Name} {pp.ProductionCatYear}"
                };
            });
            colors.AddRange(repo.GetColorDetails().Select(c => { return new Color { Id = c.ColorId, Name = c.Name }; }));
            return Json(new
            {
                material = mats.Select(r => r.Name),
                colors = colors.Select(r => r.Name),
                sizes = sizes.Select(r =>  { return new { r.Id, r.Name }; } ),
                markers = markers.Select(r => r.Name),
                plannedProductions

            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetValidatoinListsWithIds()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var mats = repo.GetMaterials();
            var sizes = repo.GetSizes();
            var markers = repo.GetMarkerCatergorys();
            var colors = repo.GetColors().ToList();
            colors.AddRange(repo.GetColorDetails().Select(c => { return new Color { Id = c.ColorId, Name = c.Name }; }));
            return Json(new
            {
                material = mats.Select(r => { return new { r.Id, r.Name }; }),
                colors = colors.Select(r => { return new { r.Id, r.Name }; }),
                sizes = sizes.Select(r => { return new { r.Id, r.Name }; }),
                markers = markers.Select(r => { return new { r.Id, r.Name }; }),
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAtributteListsForFilter()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var mats = repo.GetMaterials();
            var sizes = repo.GetSizes();
            var markers = repo.GetMarkerCatergorys();
            var colors = repo.GetColors().ToList();
            var bodyStyle = repo.GetBodyStyles();
            var sleeve = repo.GetSleeves();
            var departments = repo.GetDepartments();
            //colors.AddRange(repo.GetColorDetails().Select(c => { return new Color { Id = c.ColorId, Name = c.Name }; }));
            return Json(new
            {
                material = mats.Select          (r => { return new { r.Id, r.Name, Selected = false }; }),
                colors = colors.Select          (r => { return new { r.Id, r.Name, Selected = false }; }),
                sizes = sizes.Select            (r => { return new { r.Id, r.Name, Selected = false }; }),
                markers = markers.Select        (r => { return new { r.Id, r.Name, Selected = false }; }),
                bodyStyles = bodyStyle.Select   (r => { return new { r.Id, r.Name, Selected = false }; }),
                sleeves = sleeve.Select         (r => { return new { r.Id, r.Name, Selected = false }; }),
                departments = departments.Select(r => { return new { r.Id, r.Name, Selected = false }; }),


            }, JsonRequestBehavior.AllowGet);
        }





        //RECIVED ITEMS

        [HttpPost]
        public ActionResult GetItemsFromLotNumbers(List<int> cuttingInstructionIds)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            var result = repo.CuttingInstructionItemsWithQuantityReciveds(cuttingInstructionIds);
            return Json(result);
        }

        public ActionResult ReciveItems()
        {
            return View();
        }

        public ActionResult GetLotNumbers()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            return Json(repo.GetNotCompleteInstructionForLots().Select(ct => {
                return new
                {
                    id = ct.Id,
                    text = ct.LotNumber
                };
            }),JsonRequestBehavior.AllowGet);
        }





        //PLANNED PRODUCTIONS
        public ActionResult PlannedProduction()
        {
            ViewBag.Message = TempData["Message"] != null ? TempData["Message"] : null;
            return View();
        }
        [HttpPost]
        public ActionResult GetSkusFromWizard(PlanedProductionWizardItem[] items, int MarkerId)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var marker = repo.GetMarkerCategory(MarkerId);
            if (marker != null)
            {
                var finalItems = items.Select(i =>
                {
                    return new FinalItem
                    {
                        Item = new Item
                        {
                            ColorId = i.ColorId,
                            SizeId = i.SizeId,
                            MaterialId = i.MaterialId,
                            BodyStyleId = marker.BodyStyleId,
                            SleeveId = marker.SleeveId,
                            DepartmentId = marker.DepartmentId
                        },
                        Quantity = i.Quantity
                    };
                });
                finalItems = finalItems.Select(i =>
                {
                    var item = repo.GetItem(i.Item);
                    if (item != null)
                    {
                        i.Item = item;
                        return i;
                    }
                    return null;
                });
                return Json(finalItems.Where(i => i != null).Select(i =>
                {
                    return new
                    {
                        i.Item.Id,
                        i.Item.SKU,
                        i.Quantity
                    };
                }));
            }
            return null;
        }

        [HttpPost]
        public void SubmitPlannedProduction(PlannedProduction plannedProduction, IEnumerable<PlannedProductionDetail> items)
        {

            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            //if it's a new prdoduction then we add a new one
            if (plannedProduction.Id == 0)
                repo.AddPlannedProduction(plannedProduction);

            items = items.Select(i =>
            {
                i.PlannedProductionId = plannedProduction.Id;
                return i;
            });
            repo.AddPlannedProductionDetails(items);
            var pc = repo.GetProductionCatergory(plannedProduction.ProductionCatergoryId);
            TempData["Message"] = $"You succsesfully added a new season planned production for {pc.Name} {plannedProduction.ProductionCatYear} <br/>" +
                $"With {items.Count()} Items : Total Quantity {items.Sum(i => i.Quantity)}";
        }

        [HttpPost]
        public void UpdatePlannedProductionDetails(PlannedProductionDetail plannedProductionDetail)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var currentItem = repo.GetPlannedProductionDetail(plannedProductionDetail.Id);
            plannedProductionDetail.CreatedOn = currentItem.CreatedOn;
            repo.AddNewUpdateHistory(repo.GetPlannedProductionDetail(plannedProductionDetail.Id));
            
            repo.UpdatePlannedProductionDetail(plannedProductionDetail);
        }

        [HttpPost]
        public void DeletePlannedProductionDetails(int plannedProductionDetailId)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var pp = repo.GetPlannedProductionDetail(plannedProductionDetailId);
            //repo.DeletePlannedProductionDetail(new PlannedProductionDetail
            //{
            //    Id = pp.Id,
            //    ItemId = pp.ItemId,
            //    Quantity = pp.Quantity,
            //    Deleted = true
            //});
            repo.AddNewUpdateHistory(repo.GetPlannedProductionDetail(plannedProductionDetailId), "deleted");
            repo.DeletePlannedProductionDetail(plannedProductionDetailId);
        }

        [HttpPost]
        public void UpdatePlannedProductionNotes(PlannedProduction plannedProduction)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var currentItem = JsonConvert.DeserializeObject<PlannedProduction>(repo.GetPlannedProduction(plannedProduction.Id).GetBasePropertiesOnDbObject());
            repo.AddNewUpdateHistory(currentItem);
            currentItem.Notes = plannedProduction.Notes;

            repo.UpdatePlannedProduction(currentItem);
        }

        [HttpPost]
        public void DeletePlannedProduction(int plannedProductionId)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            repo.AddNewUpdateHistory(repo.GetPlannedProduction(plannedProductionId),"deleted");
            repo.DeletePlannedProduction(plannedProductionId);
        }

        public ActionResult GetPlannedProductions()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            return Json(repo.GetPlannedProductions().Select(pp =>
            {
                return new
                {
                    pp.Id,
                    Name = $"{pp.ProductionCatergory.Name} {pp.ProductionCatYear}"
                };
            }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPlannedProduction(PlannedProduction plannedProduction)
        {
            var list = new List<string>();
            var test = list.NotNUllOrEmpty();
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var pp = repo.GetPlannedProductionWithDetails(plannedProduction);
            return Json(pp != null ? new
            {
                pp.Id,
                pp.ProductionCatergoryId,
                pp.ProductionCatYear,
                Items = pp.PlannedProductionDetails.NotNUllOrEmpty() ? pp.PlannedProductionDetails.Select(p => {
                    return new
                    {
                        p.Id,
                        ItemId = p.Item.Id,
                        p.Item.SKU,
                        p.Quantity,
                        Edit = false
                    };
                }) : null
            } : null);
        }

        public ActionResult GetItemId(string sku)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            var item = repo.GetItem(sku);

            return item != null ? Json(new { item.Id, item.SKU }, JsonRequestBehavior.AllowGet) : null;
        }




        //FUNCTIONS

        private int MarkerId(Finalmarker marker)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var sizes = marker.Sizes.Select(s =>
            {
                return new MarkerDetail
                {
                    SizeId = s.SizeId,
                    AmountPerLayer = s.AmountPerLayer
                };
            });
            var existingMarker = repo.GetMarker(marker.MarkerCatId, sizes.ToList());
            if (existingMarker != null)
            {
                return existingMarker.Id;
            }
            else
            {
                var newMarker = new Marker { MarkerCatId = marker.MarkerCatId };
                repo.AddMarker(newMarker);
                sizes = sizes.Select(s => { s.MarkerId = newMarker.Id; return s; });
                repo.AddMarkerDetails(sizes);
                return newMarker.Id;
            }
        }

        //private int LotNumberIndex()
        //{
        //    var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
        //    return repo.LastLotNumber();
        //}

        private int LotNumberIndex()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            return repo.LotNumberIndex();
        }

        private ProductionForCT AddLotNumbers(ProductionForCT production)
        {
            production.LastLotNumber = LotNumberIndex();
            production.Markers = production.Markers.Select((m, i) => { m.LotNumber = production.LastLotNumber + i; return m; }).ToList();
            //production.Markers = production.Markers.Select((m, i) => { m.LotNumber = production.LastLotNumber + 1 + i; return m; }).ToList();
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

        private int GetFabricId(int matId, int colId)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var fabric = new Fabric { MaterialId = matId, ColorId = colId };
            var existingFabric = repo.GetFabric(fabric);
            if (existingFabric != null)
            {
                return existingFabric.Id;
            }
            else
            {
                repo.AddFabric(fabric);
                return fabric.Id;
            }
        }   




        //FILE DOWNLOAD

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

        //[HttpPost]
        //public ActionResult SubmitCT(CuttingInstruction instruction,IEnumerable<CuttingInstructionItem> items)
        //{
        //    var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
        //    repo.AddCuttingTicket(instruction);
        //    items = items.Select(i => { i.CuttingInstructionDetailsId = instruction.Id; return i; });
        //    repo.AddCTDetails(items);
        //    //TempData["Message"] = $"Sussessfully added a new cutting ticket: Id - {instruction.Id}, From date: {instruction.Date.ToShortDateString()} Lot# : {instruction.Lot_ ?? 0} => Number of items: {items.Count()}, Total items: {items.Sum(i => i.Quantity)}";
        //    Session["ItemsWithErrors"] = null;
        //    return RedirectToAction("NewProduction");
        //}



        //

        //

    }
    public static class EnumExtensions
    {
        public static string EnumToJson(this Type type)
        {
            if (!type.IsEnum)
                throw new InvalidOperationException("enum expected");

            var results =
                Enum.GetValues(type).Cast<object>()
                    .ToDictionary(enumValue => enumValue.ToString(), enumValue => (int)enumValue);


            return string.Format("{{ \"{0}\" : {1} }}", type.Name, Newtonsoft.Json.JsonConvert.SerializeObject(results));

        }
    }
}
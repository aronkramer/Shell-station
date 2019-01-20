using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Linq;
//using ProductionTracker.OldData;
using ProductionTracker.Web.Models;
using ProductionTracker.Data;

namespace ProductionTracker.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = TempData["Message"] != null ? TempData["Message"] : null;
            return View();
        }

        public ActionResult GetAllItemsWithDetails(bool isInCuttingTicket)
        {
            var ProdRepo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            //var openedCTIDs = ProdRepo.GetOpenedInstructionsIds();
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            if (isInCuttingTicket)
            {
                var itemsWithextras = repo.GetItemsInCuttingInstruction(isInCuttingTicket).Select(i =>
                {
                    return new ItemWithQuantityVM
                    {
                        Item = i,
                        Quantitys = repo.GetQuantitysPerItem(i),
                        LastCuttingInstruction = repo.LastCuttingInstruction(i)
                    };
                });
                return Json(itemsWithextras.Select(it =>
                {
                    var lastCuttingInstructionQuantitys = repo.GetQuantitysPerItemFromOpenCTs(it.Item);
                    return new
                    {
                        it.Item.Id,
                        it.Item.SKU,
                        LastCuttingInstructionDate = it.LastCuttingInstruction.Production.Date.ToShortDateString(),
                        ItemsNotReceived = (it.Quantitys.AmountOrdered - it.Quantitys.AmountReceived).ToString(),
                        PercentageRecived = string.Format("{0:P}", double.Parse(lastCuttingInstructionQuantitys.AmountReceived.ToString()) / lastCuttingInstructionQuantitys.AmountOrdered)
                        //PercentageRecived = double.Parse(it.Quantitys.AmountReceived.ToString()) / it.Quantitys.AmountOrdered
                    };
                }), JsonRequestBehavior.AllowGet);
            }
            else
            {
                //var itemsWithextras = repo.GetItemsInCuttingInstruction().Select(i =>
                //{
                //    var isInCuttingInstruction = repo.ItemExsitsInCuttingInstruction(i.Id);
                //    return new ItemWithQuantityVM
                //    {
                //        Item = i,
                //        Quantitys = isInCuttingInstruction ? repo.GetQuantitysPerItem(i) : null,
                //        LastCuttingInstruction = isInCuttingInstruction ? repo.LastCuttingInstruction(i) : null
                //    };
                //});
                //return Json(itemsWithextras.Select(it =>
                //{
                //    var isInCuttingInstruction = repo.ItemExsitsInCuttingInstruction(it.Item.Id);
                //    return new
                //    {
                //        it.Item.Id,
                //        it.Item.SKU,
                //        LastCuttingInstructionDate = isInCuttingInstruction ? it.LastCuttingInstruction.Production.Date.ToShortDateString() : "--------",
                //        ItemsNotReceived = isInCuttingInstruction ? (it.Quantitys.AmountOrdered - it.Quantitys.AmountReceived).ToString() : "--------",
                //        PercentageRecived = isInCuttingInstruction ? string.Format("{0:P}", double.Parse(it.Quantitys.AmountReceived.ToString()) / it.Quantitys.AmountOrdered) : "--------"
                //        PercentageRecived = double.Parse(it.Quantitys.AmountReceived.ToString()) / it.Quantitys.AmountOrdered
                //    };
                //}), JsonRequestBehavior.AllowGet);
                return null;
            }
        }

        public ActionResult GetAllActivityOfAItem(int id)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            var item = repo.GetItemWithActivity(id);
            var activity = new List<ItemActivity>();
            foreach (var recived in item.ReceivingItemsTransactions)
            {
                activity.Add(new ItemActivity
                {
                    Id = recived.Id,
                    Type = ActivityType.Received,
                    Date = recived.Date.ToShortDateString(),
                    Quantity = recived.Quantity,
                    CuttingInstructionId = recived.CuttingInstuctionId
                });
            }
            foreach (var CuttingInstruction in item.CuttingInstructionItems)
            {
                activity.Add(new ItemActivity
                {
                    Id = CuttingInstruction.Id,
                    Type = ActivityType.Ordered,
                    Date = CuttingInstruction.CuttingInstructionDetail.CuttingInstruction.Production.Date.ToShortDateString(),
                    Quantity = CuttingInstruction.Quantity,
                    CuttingInstructionId = CuttingInstruction.CuttingInstructionDetail.CuttingInstructionId,

                });
            }
            return Json(new
            {
                item = new
                {
                    Id = item.Id,
                    SKU = item.SKU,
                    Name = item.SKU
                },
                activity = activity
                .OrderByDescending(a => a.Date)
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCuttingInstructionsWithInfo()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var CuttingInstructions = repo.GetOpenedProductions();

            return Json(CuttingInstructions.Select(p =>
            {
                var sumor = p.CuttingInstructions.Sum(c => c.CuttingInstructionDetails.Sum(pd => pd.CuttingInstructionItems.Sum(d => d.Quantity)));
                var sumre = p.CuttingInstructions.Sum(c => c.ReceivingItemsTransactions.Sum(re => re.Quantity));
                return new
                {
                    Date = p.Date.ToShortDateString(),
                    Lot = string.Join(",", p.CuttingInstructions.Select(c => c.LotNumber)),
                    Id = p.Id,
                    TotalItems = sumor,
                    ItemsNotReceived = sumor - sumre,
                    PercentageFilled = string.Format("{0:P}", double.Parse(sumre.ToString()) / sumor)

                };
            }).OrderByDescending(p => p.Date), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDeatilsOfACuttingInstruction(int id)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var result = repo.GetProduction(id);
            var details = new List<CutingIntructionDetailsVM>();
            result.CuttingInstructions.ToList().ForEach(c => c.CuttingInstructionDetails.ToList().ForEach(pd =>
        {
            pd.CuttingInstructionItems.ToList().ForEach(pdi => 
            {

            var received = result.CuttingInstructions.Select(rt => rt.ReceivingItemsTransactions.Where(ri => ri.CuttingInstuctionId == c.Id && ri.ItemId == pdi.ItemId).Sum(ri => ri.Quantity)).Sum();

            details.Add(new CutingIntructionDetailsVM
            {
                Id = pdi.ItemId,
                SKU = pdi.Item.SKU,
                OrderedId = pdi.Id,
                Ordered = pdi.Quantity,
                Received = received,
                PercentageFilled = string.Format("{0:P}", double.Parse(received.ToString()) / pdi.Quantity),
                Lot = pd.CuttingInstruction.LotNumber,
                CuttingInstructionId = pd.CuttingInstructionId

            });

            });
        }));
            return Json(new
            {
                Production = new
                {
                    Name = $"Production from {result.Date.ToShortDateString()}",
                    Id = result.Id,
                    Date = result.Date.ToShortDateString(),
                    CuttingIntrustionIds = result.CuttingInstructions.Select(c => c.Id)

                },
                details
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void AddRecivedItems(IEnumerable<RecivingItemWithOrdered> items)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            UpdateCuttinginstructions(items);
            var itemsRecived = items.Where(i => i.Quantity != 0).Select(i =>
            {
                return new ReceivingItemsTransaction
                {
                    Adjusment = false,
                    ItemId = i.ItemId,
                    CuttingInstuctionId = i.CuttingInstuctionId,
                    Date = i.Date,
                    Quantity = i.Quantity
                };
            });
            repo.AddItemsRecived(itemsRecived);
        }

        //public ActionResult NewMarker()
        //{
        //    var colorRepo = new ColorRepository(Properties.Settings.Default.ConStr);
        //    var repo = new OldCuttingInstructionRepository(Properties.Settings.Default.ConStr);
        //    var vm = new NewMarkerVM
        //    {
        //        Departments = repo.GetDepartments(),
        //        Sleeves = repo.GetAllSleeves(),
        //        Styles = repo.GetAllStyles()
        //    };
        //    return View(vm);
        //}

        //public ActionResult GetSizesOfDepartment(int depId)
        //{
        //    var repo = new OldCuttingInstructionRepository(Properties.Settings.Default.ConStr);
        //    var sizes = repo.GetAllSizesByDepartment(depId);
        //         return Json(sizes.Select(s =>
        //    {
        //        return new
        //        {
        //            Name = s.Name,
        //            Id = s.Id
        //        };
        //    }), JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult NewCuttingInstruction()
        //{
        //    return View();
        //}

        private void UpdateCuttinginstructions(IEnumerable<RecivingItemWithOrdered> recivingItemWithOrdereds)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            var prodRepo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            foreach (var item in recivingItemWithOrdereds)
            {
                var Quantys = repo.GetQuantitysPerItemFromCT(item.ItemId, item.CuttingInstuctionId);
                if (Quantys.AmountOrdered < Quantys.AmountReceived + item.Quantity)
                {
                    prodRepo.UpdateCID(item.OrderedId, Quantys.AmountReceived + item.Quantity);
                }

            }
        }
    }

}
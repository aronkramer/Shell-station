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
            return View();
        }
        
        public ActionResult GetAllItemsWithDetails()
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            var itemsWithextras = repo.GetItemsInCuttingInstruction().Select(i => {
                var isInCuttingInstruction = repo.ItemExsitsInCuttingInstruction(i);
                return new ItemWithQuantityVM
                {
                    Item = i,
                    Quantitys = isInCuttingInstruction ? repo.GetQuantitysPerItem(i) : null,
                    LastCuttingInstructionDate = isInCuttingInstruction ? repo.LastDateOfCuttingInstruction(i) : DateTime.MinValue
             };
            });
            return Json(itemsWithextras.Select(it => 
            {
                var isInCuttingInstruction = repo.ItemExsitsInCuttingInstruction(it.Item);
                return new
                {
                    Id = it.Item.Id,
                    SKU = it.Item.SKU,
                    LastCuttingInstructionDate = isInCuttingInstruction ? it.LastCuttingInstructionDate.ToShortDateString(): "--------",
                    ItemsNotReceived = isInCuttingInstruction ? (it.Quantitys.AmountOrdered - it.Quantitys.AmountReceived).ToString() : "--------",
                    PercentageRecived = isInCuttingInstruction ? String.Format("{0:P}", double.Parse(it.Quantitys.AmountReceived.ToString()) / it.Quantitys.AmountOrdered) : "--------"
                    //PercentageRecived = double.Parse(it.Quantitys.AmountReceived.ToString()) / it.Quantitys.AmountOrdered
                };
            }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllActivityOfAItem (int id)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            var item = repo.GetItemWithActivity(id);
            var activity = new List<ItemActivity>();
            foreach (var recived in item.ReceivingItemsTransactions)
            {
                activity.Add(new ItemActivity
                {
                    Type = $"Recived",
                    Date = recived.Date.ToShortDateString(),
                    Quantity = recived.Quantity,
                    CuttingInstructionId = recived.CuttingInstuctionId
                });
            }
            foreach (var CuttingInstruction in item.CuttingInstructionDetails)
            {
                activity.Add(new ItemActivity
                {
                    Type = $"CuttingInstruction :{CuttingInstruction.CuttingInstruction.Date}",
                    Date = CuttingInstruction.CuttingInstruction.Date.ToShortDateString(),
                    Quantity = CuttingInstruction.Quantity,
                    CuttingInstructionId = CuttingInstruction.CuttingInstructionId
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
                //.Select(a =>
                //{
                //    return new
                //    {
                //        Type = a.Type,
                //        Date = a.Date.ToShortDateString(),
                //        Quantity = a.Quantity
                //    };
                //})
                .OrderByDescending(a => a.Date)
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCuttingInstructionsWithInfo()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var CuttingInstructions = repo.GetInstructions();
            
            return Json(CuttingInstructions.Select(p =>
            {
                var sumor = p.CuttingInstructionDetails.Sum(pd => pd.Quantity);
                var sumre = p.ReceivingItemsTransactions.Sum(re => re.Quantity);
                return new
                {
                    Date = p.Date.ToShortDateString(),
                    Lot = p.Lot_,
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
            var result = repo.GetInstruction(id);
            var details = result.CuttingInstructionDetails.Select(pd =>
            {
                var received = result.ReceivingItemsTransactions.Where(ri => ri.CuttingInstuctionId == result.Id && ri.ItemId == pd.ItemId).Sum(ri => ri.Quantity);
                return new
                {
                    Id = pd.ItemId,
                    SKU = pd.Item.SKU,
                    Ordered = pd.Quantity,
                    Received = received,
                    PercentageFilled = string.Format("{0:P}", double.Parse(received.ToString()) / pd.Quantity)
                };

            });
            return Json(new
            {
                CuttingInstruction = new
                {
                   Name = result.Lot_,
                   Id = result.Id,
                   Date = result.Date.ToShortDateString()
                    
                },
                details
            },JsonRequestBehavior.AllowGet);
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

        public ActionResult NewCuttingInstruction()
        {
            return View();
        }
    }
    
}
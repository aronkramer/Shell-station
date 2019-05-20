using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionTracker.Data;

namespace ProductionTracker.Web.Controllers
{
    public class TestingController : Controller
    {
        // GET: Testing
        public ActionResult Index()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            
            var test = repo.GetHistorysOfOneObject(repo.GetReceivingItemsTransaction(124));
            return View();
        }
        public ActionResult Items()
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);

            return Json(repo.GetItemsInProduction().Select(it =>
                {
                return new
                {
                    it.Item.Id,
                    it.Item.SKU,
                    it.Item.BodyStyleId,
                    it.Item.DepartmentId,
                    it.Item.MaterialId,
                    it.Item.ColorId,
                    it.Item.SleeveId,
                    it.Item.SizeId,
                    it.LastCuttingInstructionDate,
                    ItemsNotReceived = (it.Quantitys.AmountOrdered - it.Quantitys.AmountReceived).ToString(),
                };
            }), JsonRequestBehavior.AllowGet);
        }
        public ActionResult Details(int id, int? months = null)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            var item = repo.GetItemWithActivity(id, months);
            return Json(new
            {
                item = new
                {
                    Id =   item.Item.Id,
                    SKU =  item.Item.SKU,
                    Name = item.Item.SKU
                },
                activity = item.Activities
               
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ItemsInSeason (int plannedProdId)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            var seasonWithItems = repo.GetASeasonsItemsWithQuantitys(plannedProdId);
            return Json(new
            {
                seasonWithItems.Season,
                Items = seasonWithItems.ItemsWithQuantities.Select(it =>
                {
                   return new
                   {
                       it.Item.Id,
                       it.Item.SKU,
                       it.Item.BodyStyleId,
                       it.Item.DepartmentId,
                       it.Item.MaterialId,
                       it.Item.ColorId,
                       it.Item.SleeveId,
                       it.Item.SizeId,
                       it.LastCuttingInstructionDate,
                       it.Quantitys.AmountOrdered,
                       it.Quantitys.AmountReceived,
                       it.Quantitys.PlannedAmount,
                       ItemsNotReceived = (it.Quantitys.AmountOrdered - it.Quantitys.AmountReceived).ToString(),
                   };
                })
            }, JsonRequestBehavior.AllowGet);
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Linq;
using ProductionTracker.Data;
using ProductionTracker.Web.Models;

namespace ProductionTracker.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ItemAdder()
        {
            return View();
        }
        public ActionResult GetAllItemWithDetails()
        {
            var repo = new ProductionRepository(Properties.Settings.Default.ConStr);
            var itemsWithextras = repo.GetAllItemsInProduction().Select(i => { return new ItemWithQuantityVM
            {
                Item = i,
                Quantitys = repo.GetQuantitysPerItem(i),
                LastProductionDate = repo.LastDateOfProductionPerItem(i)
             };
            });
            return Json(itemsWithextras.Select(it => 
            {
                return new
                {
                    Id = it.Item.Id,
                    SKU = it.Item.SKU,
                    LastProductionDate = it.LastProductionDate.ToShortDateString(),
                    ItemsNotReceived = it.Quantitys.AmountOrdered - it.Quantitys.AmountReceived,
                    PercentageRecived = String.Format("{0:P}", double.Parse(it.Quantitys.AmountReceived.ToString()) / it.Quantitys.AmountOrdered)
                    //PercentageRecived = double.Parse(it.Quantitys.AmountReceived.ToString()) / it.Quantitys.AmountOrdered
                };
            }), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAllActivityOfAItem (int id)
        {
            var repo = new ProductionRepository(Properties.Settings.Default.ConStr);
            var item = repo.GetItemWithActivity(id);
            var activity = new List<ItemActivity>();
            foreach (var recived in item.ReceivedItems)
            {
                activity.Add(new ItemActivity
                {
                    Type = $"Recived",
                    Date = recived.Date,
                    Quantity = recived.Quantity
                });
            }
            foreach (var production in item.ProductionDetails)
            {
                activity.Add(new ItemActivity
                {
                    Type = $"Production :{production.Production.Name}",
                    Date = production.Production.Date,
                    Quantity = production.Quantity
                });
            }
            return Json(activity.Select( a=> {
                return new
                {
                    Type = a.Type,
                    Date = a.Date,
                    Quantity = a.Quantity
                };
            }
            ), JsonRequestBehavior.AllowGet);
        }

    }
}
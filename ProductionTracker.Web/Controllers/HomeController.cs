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
        
        public ActionResult GetAllItemsWithDetails()
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
                    Date = recived.Date.ToShortDateString(),
                    Quantity = recived.Quantity,
                    ProductionId = recived.ProductionId
                });
            }
            foreach (var production in item.ProductionDetails)
            {
                activity.Add(new ItemActivity
                {
                    Type = $"Production :{production.Production.Name}",
                    Date = production.Production.Date.ToShortDateString(),
                    Quantity = production.Quantity,
                    ProductionId = production.ProductionId
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

        public ActionResult GetProductionsWithInfo()
        {
            var repo = new ProductionRepository(Properties.Settings.Default.ConStr);
            var productions = repo.GetAllProductions();
            
            return Json(productions.Select(p =>
            {
                var sumor = p.ProductionDetails.Sum(pd => pd.Quantity);
                var sumre = p.ReceivedItems.Sum(re => re.Quantity);
                return new
                {
                    Date = p.Date.ToShortDateString(),
                    Name = p.Name,
                    Id = p.Id,
                    TotalItems = sumor,
                    ItemsNotReceived = sumor - sumre,
                    PercentageFilled = String.Format("{0:P}", double.Parse(sumre.ToString()) / sumor)

                };
            }).OrderByDescending(p => p.Date), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDeatilsOfAProduction(int id)
        {
            var repo = new ProductionRepository(Properties.Settings.Default.ConStr);
            var result = repo.GetProductionById(id);
            var details = result.ProductionDetails.Select(pd =>
            {
                var received = result.ReceivedItems.Where(ri => ri.ProductionId == result.Id && ri.ItemId == pd.ItemId).Sum(ri => ri.Quantity);
                return new
                {
                    Id = pd.ItemId,
                    SKU = pd.Item.SKU,
                    Ordered = pd.Quantity,
                    Received = received,
                    PercentageFilled = String.Format("{0:P}", double.Parse(received.ToString()) / pd.Quantity)
                };

            });
            return Json(new
            {
                production = new
                {
                   Name = result.Name,
                   Id = result.Id,
                   Date = result.Date.ToShortDateString()
                    
                },
                details
            },JsonRequestBehavior.AllowGet);
        }

    }
    
}
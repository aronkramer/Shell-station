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
            var repo = new ItemRepository(Properties.Settings.Default.ConStr);
            var itemsWithextras = repo.GetAllItemsInProduction().Select(i => {
                var isInProduction = repo.CheckIfItemIsInProduction(i);
                return new ItemWithQuantityVM
                {
                    Item = i,
                    Quantitys = isInProduction ? repo.GetQuantitysPerItem(i) : null,
                    LastProductionDate = isInProduction ? repo.LastDateOfProductionPerItem(i) : DateTime.MinValue
             };
            });
            return Json(itemsWithextras.Select(it => 
            {
                var isInProduction = repo.CheckIfItemIsInProduction(it.Item);
                return new
                {
                    Id = it.Item.Id,
                    SKU = it.Item.SKU,
                    LastProductionDate = isInProduction ? it.LastProductionDate.ToShortDateString(): "--------",
                    ItemsNotReceived = isInProduction ? (it.Quantitys.AmountOrdered - it.Quantitys.AmountReceived).ToString() : "--------",
                    PercentageRecived = isInProduction ? String.Format("{0:P}", double.Parse(it.Quantitys.AmountReceived.ToString()) / it.Quantitys.AmountOrdered) : "--------"
                    //PercentageRecived = double.Parse(it.Quantitys.AmountReceived.ToString()) / it.Quantitys.AmountOrdered
                };
            }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllActivityOfAItem (int id)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ConStr);
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

        public ActionResult NewMarker()
        {
            var colorRepo = new ColorRepository(Properties.Settings.Default.ConStr);
            var repo = new ProductionRepository(Properties.Settings.Default.ConStr);
            var vm = new NewMarkerVM
            {
                Departments = repo.GetDepartments(),
                Sleeves = repo.GetAllSleeves(),
                Styles = repo.GetAllStyles()
            };
            return View(vm);
        }

        public ActionResult GetSizesOfDepartment(int depId)
        {
            var repo = new ProductionRepository(Properties.Settings.Default.ConStr);
            var sizes = repo.GetAllSizesByDepartment(depId);
                 return Json(sizes.Select(s =>
            {
                return new
                {
                    Name = s.Name,
                    Id = s.Id
                };
            }), JsonRequestBehavior.AllowGet);
        }
    }
    
}
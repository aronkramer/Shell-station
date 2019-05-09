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
    }
}
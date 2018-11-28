using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProductionTracker.Web.Controllers
{
    public class ProductionController : Controller
    {
        public ActionResult NewProduction()
        {
            return View();
        }
    }
}
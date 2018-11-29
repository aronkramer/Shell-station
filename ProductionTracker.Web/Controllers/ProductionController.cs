using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionTracker.Web.Excel;
using ProductionTracker.Data;

namespace ProductionTracker.Web.Controllers
{
    public class ProductionController : Controller
    {
        public ActionResult NewProduction(ErrorsAndItems items)
        {
           
            return View(items);
        }
        [HttpPost]
        public ActionResult NewProduction(HttpPostedFileBase cuttingTicket)
        {
            var dT = ExcelActions.ConvertXSLXtoDataTable(cuttingTicket);
            var production = ExcelActions.ConvertCtToProduction(dT);
            var items = ExcelActions.ConvertProductoinToItems(production);

            return RedirectToAction("NewProduction",items);
        }
    }
}
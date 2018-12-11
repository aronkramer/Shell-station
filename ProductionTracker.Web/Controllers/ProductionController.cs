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
            var dT = ExcelActions.ConvertXSLXtoDataTable(cuttingTicket);
            var production = ExcelActions.ConvertCtToProduction(dT);
            var items = ExcelActions.ConvertProductoinToItems(production);
            Session["ItemsWithErrors"] = items;
            return RedirectToAction("NewProduction");
        }
        public ActionResult NewProductionConfimation(ErrorsAndItems items)
        {
            return View(items);
        }
        [HttpPost]
        public ActionResult SubmitCT(CuttingInstruction instruction,IEnumerable<CuttingInstructionDetail> items)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            repo.AddCuttingTicket(instruction);
            items = items.Select(i => { i.CuttingInstructionId = instruction.Id; return i; });
            repo.AddCTDetails(items);
            TempData["Message"] = $"Sussessfully added a new cutting ticket: Id - {instruction.Id}, From date: {instruction.Date.ToShortDateString()} Lot# : {instruction.Lot_ ?? 0} => Number of items: {items.Count()}, Total items: {items.Sum(i => i.Quantity)}";
            Session["ItemsWithErrors"] = null;
            return RedirectToAction("NewProduction");
        }
        public ActionResult GetItemId(string sku)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            var item = repo.GetItem(sku);

            return item != null ? Json(new { item.Id, item.SKU }, JsonRequestBehavior.AllowGet): null;
        }
    }
}
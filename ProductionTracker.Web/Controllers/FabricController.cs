using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionTracker.Data;

namespace ProductionTracker.Web.Controllers
{
    public class FabricController : Controller
    {
        // GET: Fabric
        public ActionResult ReceivedFabric()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ReceivedFabric(List<int> stuff)
        {
            if(stuff != null)
            {
                return View("ReceivedFabric", stuff);
            }
            return RedirectToAction("ReceivedFabric");
        }
        public ActionResult GetAllMaterils()
        {
            var repo = new FabricRepository(Properties.Settings.Default.ConStr);

            return Json(repo.AllMaterial().Select(m =>
            {
                return new
                {
                    Id = m.Id,
                    Name = m.Name,
                    PricePerYard = m.PricePerYard
                };
            }
                ),JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAllColors()
        {
            var repo = new ColorRepository(Properties.Settings.Default.ConStr);

            return Json(repo.GetAllColors().Select(c =>
            {
                return new
                {
                    Id = c.Id,
                    Name = c.Name
                };
            }
                ), JsonRequestBehavior.AllowGet);
        }
    }
}
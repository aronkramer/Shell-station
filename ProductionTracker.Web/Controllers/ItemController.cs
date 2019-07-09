using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Linq;
using ProductionTracker.Data;
using ProductionTracker.Web.Models;
using Newtonsoft.Json;

namespace ProductionTracker.Web.Controllers
{
    public class ItemController : Controller
    {
        // GET: Item
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ItemAdder()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var vm = new ItemAdderVM
            {
                Departments = repo.GetDepartments(),
                Materials = repo.GetMaterials(),
                Colors = repo.GetColors(),
                Sleeves = repo.GetSleeves(),
                Styles = repo.GetBodyStyles(),
                CheckedDepartments = (List<string>)Session["departments"] ?? null
            };
            return View(vm);
        }
        [HttpPost]
        public ActionResult ItemAdder(List<int> departmentIds, List<int> styles, List<int> materialIds, List<int> sleaves, List<int> colorIds)
        {
            if (!departmentIds.NotNull() || !styles.NotNull() || !materialIds.NotNull() || !sleaves.NotNull() || !colorIds.NotNull())
            {
                return RedirectToAction("ItemAdder");
            }
            var items = MakeItemsBasedOnCritera(departmentIds, styles, materialIds, sleaves, colorIds);
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            items = repo.GetUniqueItemsAndUnquieSKU(items.ToList());
            foreach (var item in items)
            {
                item.SKU = GetSku(item);
            }
            if (items != null && items.Count() >= 1)
            {
                return View("ConfirmationPage", items.OrderBy(i => i.SKU));
            }
            return RedirectToAction("ItemAdder");
        }
        [HttpPost]
        public ActionResult AddItems(List<Item> items)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            items = items.Where(i => i.SKU != null).ToList();
            items = repo.GetUniqueItemsAndUnquieSKU(items).ToList();
            repo.AddItems(items);
            return RedirectToAction("ItemAdder");
        }
        public ActionResult Colors()
        {
            return View();
        }
        
        [HttpGet]
        public ActionResult GetColors()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            return Json(repo.GetColors().Select(c =>
            {
                return JsonConvert.DeserializeObject<Color>(Helpers.GetBasePropertiesOnDbObject(c));
            }), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AddColors(List<Color> colors)
        {
            colors.RemoveAll(c => c.Id == 0 || c.Name == null);
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            repo.AddColors(colors);
            return RedirectToAction("Colors");
        }
        [HttpPost]
        public void EditColors(int ColorId, string ColorName)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            repo.EditColor(ColorId, ColorName);
        }
        [HttpPost]
        public void DeleteColors(int ColorId)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            repo.DeleteColor(ColorId);
        }
        [HttpGet]
        public ActionResult GetSleeves()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            return Json(repo.GetSleeves().Select(c =>
            {
                return JsonConvert.DeserializeObject<Sleeve>(Helpers.GetBasePropertiesOnDbObject(c));
            }), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public void UpdateSleeve(Sleeve sleeve)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            repo.EditSleeve(sleeve);
        }
        [HttpGet]
        public ActionResult GetBodyStyle()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            return Json(repo.GetBodyStyle().Select(s =>
            {
                return JsonConvert.DeserializeObject<BodyStyle>(Helpers.GetBasePropertiesOnDbObject(s));
            }), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public void UpdateBodyStyle(BodyStyle bodyStyle)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            repo.EditBodyStyle(bodyStyle);
        }
        [HttpGet]
        public ActionResult GetMaterial()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            return Json(repo.GetMaterial().Select(m =>
            {
                return JsonConvert.DeserializeObject<Material>(Helpers.GetBasePropertiesOnDbObject(m));
            }), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public void UpdateMaterial(Material material)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            repo.EditMaterial(material);
        }
        [HttpPost]
        public void DeleteItem(int id, string type)
        {
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            repo.DeleteItem(id, type);
        }
        public ActionResult AddAttributeItem(string objName, string type)
        { 
            var repo = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
            return Json(repo.AddItem(objName,type));
        }
        private IEnumerable<Item> MakeItemsBasedOnCritera(List<int> departmentIds, List<int> styles, List<int> materialIds, List<int> sleaves, List<int> colorIds)
        {
            var ItemList = new List<Item>();

            foreach (var dep in departmentIds)
            {
                foreach (var style in styles)
                {
                    if (DepartmentRuleChecker(dep, style))
                    {
                        foreach (var material in materialIds)
                        {
                            if (!(material == 1 && dep == 4))
                            {
                                foreach (var sleave in sleaves)
                                {
                                    if (SleeveRuleChecker(dep, sleave))
                                    {
                                        //var sizeList = SizeList(dep, sleave);
                                        var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
                                        var sizeList = repo.GetAllSizesByDepartment(dep);
                                        sizeList = sleave == 2 ? sizeList.Where(s => s.Id != 10 && s.Id != 11) : sizeList;
                                        foreach (var color in colorIds)
                                        {
                                            foreach (var size in sizeList)
                                            {
                                                ItemList.Add(new Item
                                                {
                                                    DepartmentId = dep,
                                                    BodyStyleId = style,
                                                    SleeveId = sleave,
                                                    ColorId = color,
                                                    SizeId = size.Id,
                                                    MaterialId = material

                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ItemList;
        }
        private bool DepartmentRuleChecker(int department, int bodyStyle)
        {
            if (department == 1 && (bodyStyle == 3 || bodyStyle == 5 || bodyStyle == 2))
            {
                return false;
            }
            else if (department == 3 && bodyStyle != 2)
            {
                return false;
            }
            else if (department == 4 && bodyStyle != 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool SleeveRuleChecker(int departemnt, int sleeve)
        {
            if (sleeve == 2 && (departemnt == 2 || departemnt == 4))
            {
                return false;
            }
            return true;
        }

        //private List<Sizes> SizeList(int depId, Sleeves sleeves)
        //{
        //    var ladiesSizes = new List<Sizes> { Sizes.Lees, Sizes.Les, Sizes.Ls, Sizes.Lm, Sizes.Ll, Sizes.Lel, Sizes.Lx, Sizes.Lxx, Sizes.Lxxx };
        //    var babySizes = new List<Sizes> { Sizes.B12_18, Sizes.B18_24, Sizes.B3_6, Sizes.B6_12 };
        //    var kidSizes = new List<Sizes> { Sizes.Kb, Sizes.Kees, Sizes.Kes, Sizes.Ks, Sizes.Km, Sizes.Kl, Sizes.Kel };
        //    var kidSizesSS = new List<Sizes> { Sizes.Kb, Sizes.Kees, Sizes.Kes, Sizes.Ks, Sizes.Km };
        //    var maternitySizes = new List<Sizes> { Sizes.Mes, Sizes.Ms, Sizes.Mm, Sizes.Ml, Sizes.Mel };
        //    if(depId == 1)
        //    {
        //        if (sleeves == Sleeves.ShortSleeve)
        //        {
        //            return kidSizesSS;
        //        }
        //        else
        //        {
        //            return kidSizes;
        //        }

        //    }
        //    else if(depId == 2)
        //    {
        //        return ladiesSizes;
        //    }
        //    else if(depId == 3)
        //    {
        //        return babySizes;
        //    }
        //    else if(depId == 4)
        //    {
        //        return maternitySizes;
        //    }
        //    return null;
        //}

        private string GetSku(Item item)
        {
            var material = Material(item.MaterialId);
            var size = Size(item.SizeId);
            var sleave = Sleave(item.SleeveId);
            var department = Department(item.DepartmentId);
            if (material == null || size == null || sleave == null || department == null)
            {
                return null;
            }
            return $"{material}{item.ColorId.ToString()}{department}{size}{sleave}".ToUpper();
        }
        private string Material(int materialId)
        {
            if (materialId == 2)
            {
                return "L";
            }
            else if (materialId == 1)
            {
                return "MD";
            }
            else if (materialId == 3)
            {
                return "CT";
            }
            return null;
        }
        private string Department(int departmentId)
        {
            if (departmentId == 1)
            {
                return "k";
            }
            else if (departmentId == 2)
            {
                return "";
            }
            else if (departmentId == 3)
            {
                return "b";
            }
            else if (departmentId == 4)
            {
                return "p";
            }
            return null;
        }
        private string Size(int size)
        {
            if (size == 5)
            {
                return "b";
            }
            else if (size == 6)
            {
                return "ess";
            }
            else if (size == 7)
            {
                return "es";
            }
            else if (size == 8)
            {
                return "s";
            }
            else if (size == 9)
            {
                return "m";

            }
            else if (size == 10)
            {
                return "l";
            }
            else if (size == 11)
            {
                return "el";
            }
            else if (size == 12)
            {
                return "x";
            }
            else if (size == 13)
            {
                return "xx";
            }
            else if (size == 14)
            {
                return "xxx";
            }
            return null;
        }
        private string Sleave(int sleeves)
        {
            if (sleeves == 1)
            {
                return "";
            }
            else if (sleeves == 2)
            {
                return "s";
            }
            else if (sleeves == 3)
            {
                return "q";
            }
            else if (sleeves == 4)
            {
                return "l";
            }
            return null;
        }
    }
    
}
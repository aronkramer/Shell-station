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
    public class ItemController : Controller
    {
        // GET: Item
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ItemAdder()
        {
            var repo = new ProductionRepository(Properties.Settings.Default.ConStr);
            var vm = new ItemAdderVM
            {
                Departments = repo.GetDepartments(),
                Fabrics = repo.GetAllFabrics(),
                Colors = repo.GetAllColors(),
                CheckedDepartments = (List<string>)Session["departments"] ?? null
            };
            return View(vm);
        }
        [HttpPost]
        public ActionResult ItemAdder(List<int> departmentIds, List<BodyStyle> styles, List<int> fabricIds, List<Sleeves> sleaves, List<int> colorIds)
        {
            var items = MakeItemsBasedOnCritera(departmentIds, styles, fabricIds, sleaves, colorIds);
            foreach (var item in items)
            {
                item.SKU = GetSku(item);
            }
            if(items != null && items.Count() >= 1)
            {
                return View("ConfirmationPage",items);
            }
            return RedirectToAction("ItemAdder");
        }
        public ActionResult Colors()
        {
            var repo = new ProductionRepository(Properties.Settings.Default.ConStr);

            return View(new ColorVM { Colors = repo.GetAllColors()});
        }
        [HttpPost]
        public ActionResult AddColors(List<Color> colors)
        {
            colors.RemoveAll(c => c.Id == 0 || c.Color1 == null);
            var repo = new ProductionRepository(Properties.Settings.Default.ConStr);
            repo.AddColors(colors);
            return RedirectToAction("Colors");
        }

        private IEnumerable<Item> MakeItemsBasedOnCritera(List<int> departmentIds, List<BodyStyle> styles, List<int> fabricIds, List<Sleeves> sleaves, List<int> colorIds)
        {
            var ItemList = new List<Item>();
            
            foreach (var dep in departmentIds)
            {
                foreach(var style in styles)
                {
                    if (DepartmentRuleChecker(dep, style))
                    {
                        foreach (var fabric in fabricIds)
                        {
                            if (!(fabric == 1 && dep == 4))
                            {
                                foreach (var sleave in sleaves)
                                {
                                    if (SleeveRuleChecker(dep, sleave))
                                    {
                                        var sizeList = SizeList(dep, sleave);
                                        foreach (var color in colorIds)
                                        {
                                            foreach(var size in sizeList)
                                            {
                                                ItemList.Add(new Item
                                                {
                                                    DepartmentId = dep,
                                                    BodyStyle = style,
                                                    Sleeve = sleave,
                                                    ColorId = color,
                                                    Size = size,
                                                    FabricId = fabric

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
        private bool DepartmentRuleChecker(int department, BodyStyle bodyStyle)
        {
            if(department == 1 && (bodyStyle == BodyStyle.CropTop || bodyStyle == BodyStyle.Dress || bodyStyle == BodyStyle.BodySuit))
            {
                return false;
            }
            else if(department == 3 && bodyStyle != BodyStyle.BodySuit)
            {
                return false;
            }
            else if(department == 4 && bodyStyle != BodyStyle.Classic)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool SleeveRuleChecker(int departemnt, Sleeves sleeve)
        {
            if(sleeve == Sleeves.ShortSleeve && (departemnt == 2 || departemnt == 4))
            {
                return false;
            }
            return true;
        }

        private List<Sizes> SizeList(int depId, Sleeves sleeves)
        {
            var ladiesSizes = new List<Sizes> { Sizes.Lees, Sizes.Les, Sizes.Ls, Sizes.Lm, Sizes.Ll, Sizes.Lel, Sizes.Lx, Sizes.Lxx, Sizes.Lxxx };
            var babySizes = new List<Sizes> { Sizes.B12_18, Sizes.B18_24, Sizes.B3_6, Sizes.B6_12 };
            var kidSizes = new List<Sizes> { Sizes.Kb, Sizes.Kees, Sizes.Kes, Sizes.Ks, Sizes.Km, Sizes.Kl, Sizes.Kel };
            var kidSizesSS = new List<Sizes> { Sizes.Kb, Sizes.Kees, Sizes.Kes, Sizes.Ks, Sizes.Km };
            var maternitySizes = new List<Sizes> { Sizes.Mes, Sizes.Ms, Sizes.Mm, Sizes.Ml, Sizes.Mel };
            if(depId == 1)
            {
                if (sleeves == Sleeves.ShortSleeve)
                {
                    return kidSizesSS;
                }
                else
                {
                    return kidSizes;
                }
                
            }
            else if(depId == 2)
            {
                return ladiesSizes;
            }
            else if(depId == 3)
            {
                return babySizes;
            }
            else if(depId == 4)
            {
                return maternitySizes;
            }
            return null;
        }

        private string GetSku(Item item)
        {
            var fabric = Fabric(item.FabricId);
            var size = Size(item.Size);
            var sleave = Sleave(item.Sleeve);
            var department = Department(item.DepartmentId);
            if (fabric == null || size == null || sleave == null || department == null)
            {
                return null;
            }
            return $"{fabric}{item.ColorId.ToString()}{department}{size}{sleave}".ToUpper();
        }
        private string Fabric(int fabricId)
        {
            if (fabricId == 2)
            {
                return "L";
            }
            else if (fabricId == 1)
            {
                return "MD";
            }
            else if (fabricId == 3)
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
            return null;
        }
        private string Size(Sizes size)
        {
            if ((int)size == 20)
            {
                return "b";
            }
            else if ((int)size == 21 || (int)size == 30)
            {
                return "ess";
            }
            else if ((int)size == 22 || (int)size == 31 || (int)size == 40)
            {
                return "es";
            }
            else if ((int)size == 23 || (int)size == 32 || (int)size == 41)
            {
                return "s";
            }
            else if ((int)size == 24 || (int)size == 33 || (int)size == 42)
            {
                return "m";

            }
            else if ((int)size == 25 || (int)size == 34 || (int)size == 43)
            {
                return "l";
            }
            else if ((int)size == 26 || (int)size == 35 || (int)size == 44)
            {
                return "el";
            }
            else if ((int)size == 36)
            {
                return "x";
            }
            else if ((int)size == 37)
            {
                return "xx";
            }
            else if ((int)size == 38)
            {
                return "xxx";
            }
            return null;
        }
        private string Sleave(Sleeves sleeves)
        {
            if ((int)sleeves == 0)
            {
                return "";
            }
            else if ((int)sleeves == 1)
            {
                return "s";
            }
            else if ((int)sleeves == 2)
            {
                return "q";
            }
            else if ((int)sleeves == 3)
            {
                return "l";
            }
            return null;
        }

    }
    
}
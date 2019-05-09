using Newtonsoft.Json;
using ProductionTracker.Data;
using System;
using System.Linq;


namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManuConst);
            var obj = repo.GetPlannedProductionDetail(1);

            repo.AddNewUpdateHistory(obj);
            //repo.AddNewUpdateHistory(new UpdateHistory
            //{
            //    Action = "updated",
            //    PropertyId = obj.Id,
            //    PropertyType = obj.GetType().Name,
            //    OldObjectData = obj.GetBasePropertiesOnDbObject()
            //});
            var historys = repo.GetUpdateHistories();

            historys.ToList().ForEach(h =>
            {
                Console.WriteLine($"{ h.PropertyType} - { h.Action} TimeStamp {h.CreatedOn} Old data --- {h.OldObjectData}");
            
            });
            var decerlized = JsonConvert.DeserializeObject (historys.ToList()[5].OldObjectData);
            Console.WriteLine("History of one item");
            var histoyOfItem = historys.Where(h => h.PropertyType == obj.GetType().Name && h.PropertyId == obj.Id).Select(h => {
                dynamic re = JsonConvert.DeserializeObject(h.OldObjectData,obj.GetType());
                re.ModifiedOn = h.CreatedOn;
                return re;
            });
            var i = 0;
            foreach(dynamic h in histoyOfItem)
            {
                i++;
                Console.WriteLine($"{i}) {h.Id} - {h.Name}");
            }
            //histoyOfItem.ToList().ForEach(h =>
            //{
            //    i++;
            //    Console.WriteLine($"{i}) {h.Id} - {h.Name}");
            //});

            //UpdateComleatedToMarkAsCompleate();
            //var repo = new ProductionRespository(Properties.Settings.Default.ManuConst);
            //var prod = repo.GetProductionForExcel(42);
            //var prodexcel = ExcelActions.ProductionToFormatForExcel(prod);
            //ExcelActions.CuttingInstruction(prodexcel);
            //Console.WriteLine(prodexcel.Name);
            //prodexcel.Markers.ForEach(m => { Console.WriteLine($"name: {m.Name} lot: {m.LotNumber}"); m.ColorMaterials.ForEach(cm => Console.WriteLine($"color: {cm.Color} color: {cm.Material} layers: {cm.Layers}")); });
            //    var repo = new ItemRepository(Properties.Settings.Default.ManuConst);
            //    repo.GetItemsInCuttingInstruction(true).ToList().ForEach(i => Console.WriteLine($"{i.Id} {i.SKU}"));
            #region Old stuff
            //var items = repo.GetAllItemsInProduction();
            //foreach(var item in items)
            //{
            //    var q = repo.GetQuantitysPerItem(item);
            //    var d = repo.LastDateOfProductionPerItem(item);
            //    Console.WriteLine($"sku:{item.SKU} items orderd{q.AmountOrdered} items recived{q.AmountReceived} last date:{d}");
            //    Console.WriteLine(String.Format("{0:P}", q.AmountReceived / q.AmountOrdered));
            //}
            //string sku = GetSku(new Item
            //{
            //    DepartmentId = 2,
            //    ColorId = 4,
            //    FabricId = 1,
            //    Sleeve = Sleeves.ShortSleeve,
            //    Size = Sizes.Ks
            //});
            //if(sku != null)
            //{
            //    Console.WriteLine(sku);
            //}
            //else
            //{
            //    Console.WriteLine("we couldnt figer out the sku, or one of the attributs dont exist the department for them");
            //}
            //var stylesNmaes = new List<string> { "3-6 Months", "6-12 Months", "12-18 Months", "18-24 Months", "B", "EES", "ES", "S", "M", "L", "EL", "X", "XX", "XXX" };
            //var result = stylesNmaes.Select(s =>
            //{
            //    return new Size
            //    {
            //        Name = s
            //    };
            //}).ToList();
            //result = repo.AddSizes(result).ToList();
            //foreach(var s in result)
            //{
            //    Console.WriteLine($"{s.Id} - {s.Name}");
            //}
            //var result = repo.GetAllSizesByDepartment(1).ToList();
            //foreach (var s in result)
            //{
            //    Console.WriteLine($"{s.Id} - {s.Name}");
            //}
            #endregion
            //Console.WriteLine("Sleeves:");
            //repo.GetAllSleeves().ToList().ForEach(s => Console.WriteLine($"Id - {s.Id} Name - {s.Name}"));
            //Console.WriteLine("Styles:");
            //repo.GetAllStyles().ToList().ForEach(s => Console.WriteLine($"Id - {s.Id} Name - {s.Name}"));
            //Console.WriteLine("Departments:");
            //repo.GetDepartments().ToList().ForEach(s => Console.WriteLine($"Id - {s.Id} Name - {s.Name}"));
            //Console.WriteLine("materials:");
            //repo.GetAllMaterials().ToList().ForEach(s => Console.WriteLine($"Id - {s.Id} Name - {s.Name}"));

            Console.ReadKey(true);
        }
        //static string GetSku (Item item)
        //{
            
        //    var fabric = Fabric(item.FabricId);
        //    var size = Size(item.Size);
        //    var sleave = Sleave(item.Sleeve);
        //    var department = Department(item.DepartmentId);
        //    if(fabric == null || size == null || sleave == null || department == null)
        //    {
        //        return null;
        //    }
        //    return $"{fabric}{item.ColorId.ToString()}{department}{size}{sleave}".ToUpper();
        //}
        //static string Fabric (int fabricId)
        //{
        //    if (fabricId == 2)
        //    {
        //        return "L";
        //    }
        //    else if(fabricId == 1)
        //    {
        //        return "MD";
        //    }
        //    else if(fabricId == 3)
        //    {
        //        return "CT";
        //    }
        //    return null;
        //}
        //static string Department(int departmentId)
        //{
        //    if(departmentId == 1)
        //    {
        //        return "k";
        //    }
        //    else if(departmentId == 2)
        //    {
        //        return "";
        //    }
        //    else if (departmentId == 3)
        //    {
        //        return "b";
        //    }
        //    return null;
        //}
        //static string Size(Sizes size)
        //{
        //    if((int)size == 20)
        //    {
        //        return "b";
        //    }
        //    else if((int)size == 21 || (int)size == 30)
        //    {
        //        return "ess";
        //    }
        //    else if ((int)size == 22 || (int)size == 31 || (int)size == 40)
        //    {
        //        return "es";
        //    }
        //    else if ((int)size == 23 || (int)size == 32 || (int)size == 41)
        //    {
        //        return "s";
        //    }
        //    else if ((int)size == 24 || (int)size == 33 || (int)size == 42)
        //    {
        //        return "m";

        //    }
        //    else if ((int)size == 25 || (int)size == 34 || (int)size == 43)
        //    {
        //        return "l";
        //    }
        //    else if ((int)size == 26 || (int)size == 35 || (int)size == 44)
        //    {
        //        return "el";
        //    }
        //    else if ((int)size == 36)
        //    {
        //        return "x";
        //    }
        //    else if ((int)size == 37)
        //    {
        //        return "xx";
        //    }
        //    else if ((int)size == 38)
        //    {
        //        return "xxx";
        //    }
        //    return null;
        //}
        //static string Sleave (Sleeves sleeves)
        //{
        //    if((int)sleeves == 0)
        //    {
        //        return "";
        //    }
        //    else if((int)sleeves == 1)
        //    {
        //        return "s";
        //    }
        //    else if ((int)sleeves == 2)
        //    {
        //        return "q";
        //    }
        //    else if ((int)sleeves == 3)
        //    {
        //        return "l";
        //    }
        //    return null;
        //}

        


        static void UpdateComleatedToMarkAsCompleate()
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManuConst);
            var listOfCompleteCTIds = repo.GetClosedInstructionsIds();
            listOfCompleteCTIds.ToList().ForEach(i =>
            {
                repo.MarkCuttingTicketAsCompleate(i);
                Console.WriteLine("sucsses");
            });
            Console.WriteLine("Done");
        }
    }
}

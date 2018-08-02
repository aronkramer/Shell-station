using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductionTracker.Data;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            var repo = new ProductionRepository(Properties.Settings.Default.ConStr);
            var items = repo.GetAllItemsInProduction();
            foreach(var item in items)
            {
                var q = repo.GetQuantitysPerItem(item);
                var d = repo.LastDateOfProductionPerItem(item);
                Console.WriteLine($"sku:{item.SKU} items orderd{q.AmountOrdered} items recived{q.AmountReceived} last date:{d}");
                Console.WriteLine(String.Format("{0:P}", q.AmountReceived / q.AmountOrdered));
            }
            Console.ReadKey(true);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionTracker.OldData
{
    public class ItemRepository
    {
        private string _connectionString;
        public ItemRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public void AddItem(Item item)
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                context.Items.InsertOnSubmit(item);
                context.SubmitChanges();
            }
        }

        public IEnumerable<Item> GetAllItemsInProduction()
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                return context.Items.ToList();
            }
        }

        public ItemQuantity GetQuantitysPerItem(Item item)
        {
            using (var context = new ProductionDataContext(_connectionString))
            {


                return new ItemQuantity
                {
                    AmountOrdered = context.ProductionDetails.Where(i => i.ItemId == item.Id).Sum(p => p.Quantity),
                    AmountReceived = context.ReceivedItems.Where(i => i.ItemId == item.Id).Sum(p => p.Quantity)
                };

            }
        }

        public DateTime LastDateOfProductionPerItem(Item item)
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                return context.ProductionDetails.Where(p => p.ItemId == item.Id).OrderByDescending(p => p.Production.Date).First().Production.Date;
            }
        }

        public bool CheckIfItemIsInProduction(Item item)
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                return context.ProductionDetails.Where(p => p.ItemId == item.Id).Any();
            }
        }

        public Item GetItemWithActivity(int id)
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<Item>(i => i.ProductionDetails);
                loadOptions.LoadWith<Item>(i => i.ReceivedItems);
                loadOptions.LoadWith<ProductionDetail>(p => p.Production);
                context.LoadOptions = loadOptions;
                return context.Items.FirstOrDefault(i => i.Id == id);

            }
        }

        public IEnumerable<Item> GetUniqueItemsAndUnquieSKU(List<Item> items)
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                var uniqueItems = new List<Item>();
                foreach (var item in items)
                {
                    var testItem = context.Items.FirstOrDefault(i => (i.DepartmentId == item.DepartmentId && i.MaterialId == item.MaterialId && i.BodyStyleId == item.BodyStyleId
                     && i.ColorId == item.ColorId && i.SleeveId == item.SleeveId && i.SizeId == item.SizeId) || i.SKU == item.SKU);
                    if(testItem == null)
                    {
                        uniqueItems.Add(item);
                    }
                    //if (!context.Items.Any(i => i.DepartmentId == item.DepartmentId && i.FabricId == item.FabricId && i.BodyStyle == item.BodyStyle
                    // && i.Color == item.Color && i.Sleeve == item.Sleeve && i.Size == item.Size) || !context.Items.Any(i => i.SKU == item.SKU))
                    //{
                    //    uniqueItems.Add(item);
                    //}
                }
                return uniqueItems;
            }
        }

        public void AddItems(IEnumerable<Item> items)
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                context.Items.InsertAllOnSubmit(items);
                context.SubmitChanges();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;

namespace ProductionTracker.Data
{
    public class ProductionRepository
    {
        private string _connectionString;
        public ProductionRepository(string connectionString)
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
    }
}


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

        public IEnumerable<Production> GetAllProductions()
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<Production>(p => p.ProductionDetails);
                loadOptions.LoadWith<Production>(p => p.ReceivedItems);
                context.LoadOptions = loadOptions;
                return context.Productions.ToList();
            }
        }

        public Production GetProductionById(int id)
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<Production>(p => p.ProductionDetails);
                loadOptions.LoadWith<Production>(p => p.ReceivedItems);
                loadOptions.LoadWith<ReceivedItem>(r => r.Item);
                loadOptions.LoadWith<ProductionDetail>(pd => pd.Item);
                context.LoadOptions = loadOptions;
                return context.Productions.FirstOrDefault(i => i.Id == id);
            }
        }

        public IEnumerable<Department> GetDepartments()
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                return context.Departments.ToList();
            }
        }
        public void AddColors(IEnumerable<Color> colors)
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                //foreach(var color in colors)
                //{
                //    context.Colors.InsertOnSubmit(color);
                //    context.SubmitChanges();
                //}
                context.Colors.InsertAllOnSubmit(colors);
                context.SubmitChanges();
            }
        }
        public IEnumerable<Color> GetAllColors()
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                return context.Colors.ToList();
            }
        }
        public IEnumerable<Fabric> GetAllFabrics()
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                return context.Fabrics.ToList();
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionTracker.Data
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
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.Items.InsertOnSubmit(item);
                context.SubmitChanges();
            }
        }

        public IEnumerable<Item> GetItemsInCuttingInstruction()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Items.ToList();
            }
        }
        public ItemQuantity GetQuantitysPerItem(Item item)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {


                return new ItemQuantity
                {
                    AmountOrdered = context.CuttingInstructionDetails.Where(i => i.ItemId == item.Id) != null ? context.CuttingInstructionDetails.Where(i => i.ItemId == item.Id).Sum(p => p.Quantity): 0 ,
                    AmountReceived = context.ReceivingItemsTransactions.Where(i => i.ItemId == item.Id) != null ? context.CuttingInstructionDetails.Where(i => i.ItemId == item.Id).Sum(p => p.Quantity) : 0
                };

            }
        }

        public DateTime LastDateOfCuttingInstruction(Item item)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.CuttingInstructionDetails.Where(p => p.ItemId == item.Id).OrderByDescending(p => p.CuttingInstruction.Date).First().CuttingInstruction.Date;
            }
        }

        public bool ItemExsitsInCuttingInstruction(Item item)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.CuttingInstructionDetails.Where(p => p.ItemId == item.Id).Any();
            }
        }

        public Item GetItemWithActivity(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<Item>(i => i.CuttingInstructionDetails);
                loadOptions.LoadWith<Item>(i => i.ReceivingItemsTransactions);
                loadOptions.LoadWith<CuttingInstruction>(p => p.CuttingInstructionDetails);
                context.LoadOptions = loadOptions;
                return context.Items.FirstOrDefault(i => i.Id == id);

            }
        }

        public IEnumerable<Item> GetUniqueItemsAndUnquieSKU(List<Item> items)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var uniqueItems = new List<Item>();
                foreach (var item in items)
                {
                    var testItem = context.Items.FirstOrDefault(i => (i.DepartmentId == item.DepartmentId && i.MaterialId == item.MaterialId && i.BodyStyleId == item.BodyStyleId
                     && i.ColorId == item.ColorId && i.SleeveId == item.SleeveId && i.SizeId == item.SizeId) || i.SKU == item.SKU);
                    if (testItem == null)
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
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.Items.InsertAllOnSubmit(items);
                context.SubmitChanges();
            }
        }

    }
}

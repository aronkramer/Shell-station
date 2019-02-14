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
        
        public IEnumerable<ItemsForBarcodes2Result> GetItemsForBarcodes()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.ItemsForBarcodes2().ToList();
            }
        }

        public IEnumerable<CuttingInstructionItem> GetItemsForBarcodes(int productionId)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.CuttingInstructionItems.Where(p => p.CuttingInstructionDetail.CuttingInstruction.ProductionId == productionId).ToList();
            }
        }

        public IEnumerable<Item> GetItemsInCuttingInstruction()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Items.ToList();
            }
        }

        public IEnumerable<Item> GetItemsInCuttingInstruction(bool isInCuttingTicket)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {

                return isInCuttingTicket ? context.CuttingInstructionItems.Select(i => i.Item).Distinct().ToList() : GetItemsInCuttingInstruction();
            }
        }

        public IEnumerable<ItemWithQuantity> GetItemsWithQuantitys()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {

                return  context.CuttingInstructionItems.Select(i => i.Item)
                    .Distinct().ToList()
                    .Select(it =>
                    {
                        return new ItemWithQuantity
                        {
                            Item = it,
                            LastCuttingInstructionDate = it.CuttingInstructionItems.Where(p => p.ItemId == it.Id).OrderByDescending(p => p.CuttingInstructionDetail.CuttingInstruction.Production.Date).First().CuttingInstructionDetail.CuttingInstruction.Production.Date.ToShortDateString(),
                            Quantitys = new ItemQuantity
                            {
                                AmountOrdered = it.CuttingInstructionItems.Where(i => i.ItemId == it.Id).Sum(p => p.Quantity),
                                AmountReceived = it.ReceivingItemsTransactions.Where(i => i.ItemId == it.Id).Count() > 0 ? it.ReceivingItemsTransactions.Where(i => i.ItemId == it.Id).Sum(p => p.Quantity) : 0
                            }

                    };
                    }).ToList();
            }
        }

        public IEnumerable<CuttingInstructionItemWithQuantityRecived> CuttingInstructionItemsWithQuantityReciveds(List<int> cuttingInstructionIds)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var list = context.CuttingInstructions.Where(c => cuttingInstructionIds.Contains(c.Id)).ToList();
                 return list.SelectMany(ct => ct.CuttingInstructionDetails).SelectMany(cid => cid.CuttingInstructionItems).Select(cii => 
                    {
                        return new CuttingInstructionItemWithQuantityRecived
                        {
                            Id = cii.Id,
                            LotNumber = cii.CuttingInstructionDetail.CuttingInstruction.LotNumber,
                            CuttingInstructionId = cii.CuttingInstructionDetail.CuttingInstructionId,
                            ItemId = cii.ItemId,
                            SKU = cii.Item.SKU,
                            QuantityOrdered = cii.Quantity,
                            QuantityReceived = cii.CuttingInstructionDetail.CuttingInstruction.ReceivingItemsTransactions
                            .Where(i => i.ItemId == cii.ItemId)
                            .Sum(i => i.Quantity)

                        };

                    }).ToList();
            }
        }

        public ItemQuantity GetQuantitysPerItem(Item item)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return new ItemQuantity
                {
                    AmountOrdered = context.CuttingInstructionItems.Where(i => i.ItemId == item.Id) != null ? context.CuttingInstructionItems.Where(i => i.ItemId == item.Id).Sum(p => p.Quantity) : 0,
                    AmountReceived = context.ReceivingItemsTransactions.Where(i => i.ItemId == item.Id).Count() > 0 ? context.ReceivingItemsTransactions.Where(i => i.ItemId == item.Id).Sum(p => p.Quantity) : 0
                };
            }
        }

        public ItemQuantity GetQuantitysPerItemFromOpenCTs(Item item, List<int> openedCTIDs)
        {

            using (var context = new ManufacturingDataContext(_connectionString))
            {

                return new ItemQuantity
                {
                    AmountOrdered = context.CuttingInstructionItems.Where(i => i.ItemId == item.Id && openedCTIDs.Contains(i.CuttingInstructionDetail.CuttingInstructionId)).Count() > 0 ? context.CuttingInstructionItems.Where(i => i.ItemId == item.Id && openedCTIDs.Contains(i.CuttingInstructionDetail.CuttingInstructionId)).Sum(p => p.Quantity) : 0,
                    AmountReceived = context.ReceivingItemsTransactions.Where(i => i.ItemId == item.Id && openedCTIDs.Contains(i.CuttingInstuctionId)).Count() > 0 ? context.ReceivingItemsTransactions.Where(i => i.ItemId == item.Id && openedCTIDs.Contains(i.CuttingInstuctionId)).Sum(p => p.Quantity) : 0
                };
            }
        }

        public ItemQuantity GetQuantitysPerItemFromOpenCTs(Item item)
        {

            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var openedCTIDs = context.CuttingInstructions
                    .Where(i => (i.CuttingInstructionDetails
                .Count() > 0 ? i.CuttingInstructionDetails
                .Sum(co => co.CuttingInstructionItems.Sum(d => d.Quantity)) : 0)
                != (i.ReceivingItemsTransactions
                .Count() > 0 ? i.ReceivingItemsTransactions.Sum(d => d.Quantity) : 0)).Select(ct => ct.Id);
                return new ItemQuantity
                {
                    AmountOrdered = context.CuttingInstructionItems
                    .Where(i => i.ItemId == item.Id && openedCTIDs
                    .Contains(i.CuttingInstructionDetail.CuttingInstructionId)).Count() > 0 ? 
                    context.CuttingInstructionItems
                    .Where(i => i.ItemId == item.Id && openedCTIDs
                    .Contains(i.CuttingInstructionDetail.CuttingInstructionId))
                    .Sum(p => p.Quantity) : 0,
                    AmountReceived = context.ReceivingItemsTransactions.Where(i => i.ItemId == item.Id && openedCTIDs.Contains(i.CuttingInstuctionId)).Count() > 0 ? context.ReceivingItemsTransactions.Where(i => i.ItemId == item.Id && openedCTIDs.Contains(i.CuttingInstuctionId)).Sum(p => p.Quantity) : 0
                };
            }
        }

        public ItemQuantity GetQuantitysPerItemFromNonCompleteCTs(Item item)
        {

            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var openedCTIDs = context.CuttingInstructions.Where(i => i.Completed == false).Select(ct => ct.Id);
                return new ItemQuantity
                {
                    AmountOrdered = context.CuttingInstructionItems
                    .Where(i => i.ItemId == item.Id && openedCTIDs
                    .Contains(i.CuttingInstructionDetail.CuttingInstructionId)).Count() > 0 ?
                    context.CuttingInstructionItems
                    .Where(i => i.ItemId == item.Id && openedCTIDs
                    .Contains(i.CuttingInstructionDetail.CuttingInstructionId))
                    .Sum(p => p.Quantity) : 0,
                    AmountReceived = context.ReceivingItemsTransactions.Where(i => i.ItemId == item.Id && openedCTIDs.Contains(i.CuttingInstuctionId)).Count() > 0 ? context.ReceivingItemsTransactions.Where(i => i.ItemId == item.Id && openedCTIDs.Contains(i.CuttingInstuctionId)).Sum(p => p.Quantity) : 0
                };
            }
        }

        public ItemQuantity GetQuantitysPerItemFromCT(int Id, int cuttingTicketId)
        {

            using (var context = new ManufacturingDataContext(_connectionString))
            {

                return new ItemQuantity
                {
                    AmountOrdered = context.CuttingInstructionItems
                    .Where(i => i.ItemId == Id && i.CuttingInstructionDetail.CuttingInstructionId == cuttingTicketId).Count() > 0 ? context.CuttingInstructionItems
                    .Where(i => i.ItemId == Id && i.CuttingInstructionDetail.CuttingInstructionId == cuttingTicketId)
                    .Sum(p => p.Quantity) : 0,
                    AmountReceived = context.ReceivingItemsTransactions.Where(i => i.ItemId == Id && i.CuttingInstuctionId == cuttingTicketId).Count() > 0 ? context.ReceivingItemsTransactions.Where(i => i.ItemId == Id && i.CuttingInstuctionId == cuttingTicketId).Sum(p => p.Quantity) : 0
                };
            }
        }

        public CuttingInstruction LastCuttingInstruction(Item item)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<CuttingInstruction>(p => p.Production);
                context.LoadOptions = loadOptions;
                return context.CuttingInstructionItems.Where(p => p.ItemId == item.Id).OrderByDescending(p => p.CuttingInstructionDetail.CuttingInstruction.Production.Date).First().CuttingInstructionDetail.CuttingInstruction;
            }
        }

        //public IEnumerable<CuttingInstruction> LastCuttingInstruction(Item item, List<int> openedCTIDs)
        //{
        //    using (var context = new ManufacturingDataContext(_connectionString))
        //    {
        //        return context.CuttingInstructionDetails.Where(p => p.ItemId == item.Id && openedCTIDs.Contains(p.CuttingInstructionId)).OrderByDescending(p => p.CuttingInstruction.Production.Date).Select(p => p.CuttingInstruction).ToList();
        //    }
        //}

        //public bool ItemExsitsInCuttingInstruction(int id)
        //{
        //    using (var context = new ManufacturingDataContext(_connectionString))
        //    {
        //        return context.CuttingInstructionItems.Where(p => p.ItemId == id).Any();
        //    }
        //}

        public Item GetItemWithActivity(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<Item>(i => i.CuttingInstructionItems);
                loadOptions.LoadWith<Item>(i => i.ReceivingItemsTransactions);
                loadOptions.LoadWith<CuttingInstructionItem>(p => p.CuttingInstructionDetail);
                loadOptions.LoadWith<CuttingInstructionDetail>(p => p.CuttingInstruction);
                loadOptions.LoadWith<CuttingInstruction>(p => p.Production);
                context.LoadOptions = loadOptions;
                return context.Items.FirstOrDefault(i => i.Id == id);

            }
        }

        //public IEnumerable<Item> GetUniqueItemsAndUnquieSKU(List<Item> items)
        //{
        //    using (var context = new ManufacturingDataContext(_connectionString))
        //    {
        //        var uniqueItems = new List<Item>();
        //        foreach (var item in items)
        //        {
        //            var testItem = context.Items.FirstOrDefault(i => (i.DepartmentId == item.DepartmentId && i.MaterialId == item.MaterialId && i.BodyStyleId == item.BodyStyleId
        //             && i.ColorId == item.ColorId && i.SleeveId == item.SleeveId && i.SizeId == item.SizeId) || i.SKU == item.SKU);
        //            if (testItem == null)
        //            {
        //                uniqueItems.Add(item);
        //            }
        //            if (!context.Items.Any(i => i.DepartmentId == item.DepartmentId && i.FabricId == item.FabricId && i.BodyStyle == item.BodyStyle
        //             && i.Color == item.Color && i.Sleeve == item.Sleeve && i.Size == item.Size) || !context.Items.Any(i => i.SKU == item.SKU))
        //            {
        //                uniqueItems.Add(item);
        //            }
        //        }
        //        return uniqueItems;
        //    }
        //}

        //public void AddItems(IEnumerable<Item> items)
        //{
        //    using (var context = new ManufacturingDataContext(_connectionString))
        //    {
        //        context.Items.InsertAllOnSubmit(items);
        //        context.SubmitChanges();
        //    }
        //}

        public Item GetItem(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Items.FirstOrDefault(i => i.Id == id);
            }
        }
        public Item GetItem(string sku)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Items.FirstOrDefault(i => i.SKU == sku.ToUpper());
            }
        }

        public IEnumerable<Item> GetItems()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Items.ToList();
            }
        }
    }
}

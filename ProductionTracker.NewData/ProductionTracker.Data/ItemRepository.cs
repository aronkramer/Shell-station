using ProductionTracker.Data.Models;
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
                            LastCuttingInstructionDatePretty = it.CuttingInstructionItems.Where(p => p.ItemId == it.Id).OrderByDescending(p => p.CuttingInstructionDetail.CuttingInstruction.Production.Date).First().CuttingInstructionDetail.CuttingInstruction.Production.Date.ToShortDateString(),
                            Quantitys = new ItemQuantity
                            {
                                AmountOrdered = it.CuttingInstructionItems.Where(i => i.ItemId == it.Id).Sum(p => p.Quantity),
                                AmountReceived = it.ReceivingItemsTransactions.Where(i => i.ItemId == it.Id).Count() > 0 ? it.ReceivingItemsTransactions.Where(i => i.ItemId == it.Id).Sum(p => p.Quantity) : 0
                            }

                    };
                    }).ToList();
            }
        }

        public IEnumerable<ItemWithQuantity> GetItemsInProduction()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadoptions = new DataLoadOptions();
                loadoptions.LoadWith<CuttingInstructionItem>(c => c.Item);
                context.LoadOptions = loadoptions;
                return context.CuttingInstructionItems.GroupBy(i => i.ItemId).Where(ite =>
                    ite.Sum(p => p.Quantity) - (ite.FirstOrDefault().Item.ReceivingItemsTransactions.Where(i => i.ItemId == ite.FirstOrDefault().ItemId).Count() > 0 ? ite.FirstOrDefault().Item.ReceivingItemsTransactions.Where(i => i.ItemId == ite.FirstOrDefault().ItemId).Sum(p => p.Quantity) : 0) > 0
                )
                    .ToList()
                    .Select(ite =>
                    {
                        var it = ite.FirstOrDefault();
                        return new ItemWithQuantity
                        {
                            Item = it.Item,
                            LastCuttingInstructionDate = ite.OrderByDescending(p => p.CuttingInstructionDetail.CuttingInstruction.Production.Date).Select(p => p.CuttingInstructionDetail.CuttingInstruction.Production.Date).FirstOrDefault(),
                            LastCuttingInstructionDatePretty = ite.OrderByDescending(p => p.CuttingInstructionDetail.CuttingInstruction.Production.Date).Select(p => p.CuttingInstructionDetail.CuttingInstruction.Production.Date).FirstOrDefault().ToShortDateString(),
                            Quantitys = new ItemQuantity
                            {
                                AmountOrdered = ite.Sum(p => p.Quantity),
                                AmountReceived = it.Item.ReceivingItemsTransactions.Where(i => i.ItemId == it.ItemId).Count() > 0 ? it.Item.ReceivingItemsTransactions.Where(i => i.ItemId == it.ItemId).Sum(p => p.Quantity) : 0
                            }

                        };
                    }).ToList();
            }
        }

        public int CurrentSeason()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Settings.FirstOrDefault().CurrentSeason;
            }
        }

        public SeasonWithItems GetASeasonsItemsWithQuantitys(int plannedProdId)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadoptions = new DataLoadOptions();
                loadoptions.LoadWith<CuttingInstructionItem>(c => c.Item);
                loadoptions.LoadWith<PlannedProductionDetail>(c => c.Item);
                context.LoadOptions = loadoptions;
                
                var plannedProdsItems = context.CuttingInstructionItems.Where(ci => ci.CuttingInstructionDetail.CuttingInstruction.PlannedProductionId == plannedProdId).GroupBy(i => i.ItemId)
                    .ToList()
                    .Select(ite =>
                    {
                        var it = ite.FirstOrDefault();
                        var plannedAmount = it.CuttingInstructionDetail.CuttingInstruction.PlannedProduction.PlannedProductionDetails.Where(p => !p.Deleted).FirstOrDefault(i => i.ItemId == it.ItemId);
                        return new ItemWithQuantity
                        {
                            Item = it.Item,
                            LastCuttingInstructionDate = ite.OrderByDescending(p => p.CuttingInstructionDetail.CuttingInstruction.Production.Date).Select(p => p.CuttingInstructionDetail.CuttingInstruction.Production.Date).FirstOrDefault(),
                            LastCuttingInstructionDatePretty = ite.OrderByDescending(p => p.CuttingInstructionDetail.CuttingInstruction.Production.Date).Select(p => p.CuttingInstructionDetail.CuttingInstruction.Production.Date).FirstOrDefault().ToShortDateString(),
                            Quantitys = new ItemQuantity
                            {
                                PlannedAmount = plannedAmount != null? plannedAmount.Quantity : 0,
                                AmountOrdered = ite.Sum(p => p.Quantity),
                                AmountReceived = it.Item.ReceivingItemsTransactions.Where(i => i.CuttingInstruction.PlannedProductionId == plannedProdId && i.ItemId == it.ItemId).Count() > 0 ? it.Item.ReceivingItemsTransactions.Where(i => i.CuttingInstruction.PlannedProductionId == plannedProdId && i.ItemId == it.ItemId).Sum(p => p.Quantity) : 0

                            }

                        };
                    }).ToList();
                var plannedProd = context.PlannedProductions.FirstOrDefault(pp => pp.Id == plannedProdId);
                var plannedProdsItems2 = plannedProd.PlannedProductionDetails.Where(p => !p.Deleted).Where(ppd => !plannedProdsItems.Select(i => i.Item.Id).Contains(ppd.ItemId))
                    .Select(ppd =>
                    {
                        return new ItemWithQuantity
                        {
                            Item = ppd.Item,
                            Quantitys = new ItemQuantity
                            {
                                PlannedAmount = ppd.Quantity
                            }
                        };
                    });
                   

            return new SeasonWithItems
            {
                ItemsWithQuantities = plannedProdsItems.Concat(plannedProdsItems2).OrderBy(i => i.Item.SKU).ToList(),
                Season = new Season { PlannedProductionId = plannedProd.Id, Name = $"{plannedProd.ProductionCatergory.Name} {plannedProd.ProductionCatYear}" }
            };
        
            }
        }

        public IEnumerable<ItemWithQuantity> GetItemsWithQuantitys(bool newway)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadoptions = new DataLoadOptions();
                loadoptions.LoadWith<CuttingInstructionItem>(c => c.Item);
                context.LoadOptions = loadoptions;
                var seasons =  context.PlannedProductions.Where(pp => !pp.Archived).OrderByDescending(s => s.CreatedOn).Take(2).SelectMany( p=> p.CuttingInstructions.SelectMany(pd => pd.CuttingInstructionDetails.SelectMany(pdi => pdi.CuttingInstructionItems)));
                //var random = context.CuttingInstructions.Where(ci => !ci.Completed && ci.PlannedProductionId == null).SelectMany(c => c.CuttingInstructionDetails.SelectMany(ci => ci.CuttingInstructionItems));
                var random = context.CuttingInstructions.Where(i => (i.CuttingInstructionDetails
                .Count() > 0 ? i.CuttingInstructionDetails
                .Sum(co => co.CuttingInstructionItems.Sum(d => d.Quantity)) : 0)
                != (i.ReceivingItemsTransactions
                .Count() > 0 ? i.ReceivingItemsTransactions.Sum(d => d.Quantity) : 0) && i.PlannedProductionId == null).SelectMany(c => c.CuttingInstructionDetails.SelectMany(ci => ci.CuttingInstructionItems));
                return seasons.Concat(random).GroupBy(i => i.ItemId)
                    .ToList()
                    .Select(ite =>
                    {
                        var it = ite.FirstOrDefault();
                        return new ItemWithQuantity
                        {
                            Item =  it.Item,
                            LastCuttingInstructionDatePretty = ite.OrderByDescending(p => p.CuttingInstructionDetail.CuttingInstruction.Production.Date).Select(p => p.CuttingInstructionDetail.CuttingInstruction.Production.Date).FirstOrDefault().ToShortDateString(), 
                            Quantitys = new ItemQuantity
                            {
                                AmountOrdered = ite.Sum(p => p.Quantity),
                                AmountReceived = it.Item.ReceivingItemsTransactions.Where(i => i.ItemId == it.Id).Count() > 0 ? it.Item.ReceivingItemsTransactions.Where(i => i.ItemId == it.Id).Sum(p => p.Quantity) : 0
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

        public ItemWithActivity GetItemWithActivity(int id,int? months = null)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                
               
                var item = context.Items.FirstOrDefault(i => i.Id == id);
                var ordered = item.CuttingInstructionItems.Where(ci => months != null ? ci.CuttingInstructionDetail.CuttingInstruction.Production.Date > DateTime.Now.AddMonths(-(int)months) : true)
                //var ordered = item.CuttingInstructionItems.Where(ci => months != null ? ci.CreatedOn > DateTime.Now.AddMonths(-(int)months) : true)
                    .Select(ci =>
                    {
                        return new ItemActivity
                        {
                            Id = ci.Id,
                            Type = ActivityType.Ordered,
                            Date = ci.CuttingInstructionDetail.CuttingInstruction.Production.Date,
                            DatePretty = ci.CuttingInstructionDetail.CuttingInstruction.Production.Date.ToShortDateString(),
                            Quantity = ci.Quantity,
                            CuttingInstructionId = ci.CuttingInstructionDetail.CuttingInstructionId,
                            Season = new Season
                            {
                                PlannedProductionId = ci.CuttingInstructionDetail.CuttingInstruction.PlannedProductionId,
                                Name = ci.CuttingInstructionDetail.CuttingInstruction.PlannedProductionId != null ? $"{ci.CuttingInstructionDetail.CuttingInstruction.PlannedProduction.ProductionCatergory.Name} {ci.CuttingInstructionDetail.CuttingInstruction.PlannedProduction.ProductionCatYear}" : "Random"
                            }
                        };
                    });
                var recived = item.ReceivingItemsTransactions.Where(ci => months != null ? ci.Date > DateTime.Now.AddMonths(-(int)months) : true)
                    .Select(ci =>
                    {
                        return new ItemActivity
                        {
                            Id = ci.Id,
                            Type = ActivityType.Received,
                            Date = ci.Date,
                            DatePretty = ci.Date.ToShortDateString(),
                            Quantity = ci.Quantity,
                            CuttingInstructionId = ci.CuttingInstruction.Id,
                            Season = new Season
                            {
                                PlannedProductionId = ci.CuttingInstruction.PlannedProductionId,
                                Name = ci.CuttingInstruction.PlannedProductionId != null ? $"{ci.CuttingInstruction.PlannedProduction.ProductionCatergory.Name} {ci.CuttingInstruction.PlannedProduction.ProductionCatYear}" : "Random"
                            }
                        };
                    });

                return new ItemWithActivity
                {
                    Item = item,
                    Activities = ordered.Concat(recived).OrderByDescending(a => a.Date).ToList()
                };

            }
        }

        public SeasonItemWithActivity GetSeasonItemWithActivity (int ppId, int itemId)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var season = context.PlannedProductions.FirstOrDefault(pp => pp.Id == ppId);
                var item = context.Items.FirstOrDefault(i => i.Id == itemId);
                var ordered = item.CuttingInstructionItems.Where(ci => ci.CuttingInstructionDetail.CuttingInstruction.PlannedProductionId == ppId)
                    .Select(ci =>
                    {
                        return new ItemActivity
                        {
                            Id = ci.Id,
                            Type = ActivityType.Ordered,
                            Date = ci.CuttingInstructionDetail.CuttingInstruction.Production.Date,
                            DatePretty = ci.CuttingInstructionDetail.CuttingInstruction.Production.Date.ToShortDateString(),
                            Quantity = ci.Quantity,
                            CuttingInstructionId = ci.CuttingInstructionDetail.CuttingInstructionId,
                        };
                    });
                var recived = item.ReceivingItemsTransactions.Where(ci => ci.CuttingInstruction.PlannedProductionId == ppId)
                    .Select(ci =>
                    {
                        return new ItemActivity
                        {
                            Id = ci.Id,
                            Type = ActivityType.Received,
                            Date = ci.Date,
                            DatePretty = ci.Date.ToShortDateString(),
                            Quantity = ci.Quantity,
                            CuttingInstructionId = ci.CuttingInstruction.Id,
                        };
                    });

                return new SeasonItemWithActivity
                {
                    ItemWithActivity = new ItemWithActivity
                    {
                        Item = item,
                        Activities = ordered.Concat(recived).OrderByDescending(a => a.Date).ToList()
                    },
                    Season = new Season
                    {
                        PlannedProductionId = season.Id,
                        Name = $"{season.ProductionCatergory.Name} {season.ProductionCatYear}"
                    },
                    TotalQuantitys = new ItemQuantity
                    {
                        PlannedAmount = season.PlannedProductionDetails.Where(p => !p.Deleted).FirstOrDefault(p => p.ItemId == itemId) != null ? 
                        season.PlannedProductionDetails.FirstOrDefault(p => p.ItemId == itemId).Quantity
                        : 0,
                        AmountReceived = recived.Sum(r => r.Quantity),
                        AmountOrdered= ordered.Sum(r => r.Quantity)
                    }
                };

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

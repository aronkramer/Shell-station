using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionTracker.Data
{
    public class ProductionRespository
    {
        private string _connectionString;

        public ProductionRespository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Color GetColor (int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Colors.FirstOrDefault(c => c.Id == id);
            }
        }
        
        public int LastLotNumber()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.CuttingInstructions.Count() > 1 ? context.CuttingInstructions.OrderByDescending(c => c.LotNumber).First().LotNumber : 2222;
            }
        }

        public Color GetColor(string name)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Colors.FirstOrDefault(c => c.Name == name.ToUpper());
            }
        }

        public int? GetColorId(string colorName)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                if (colorName != null)
                {
                    var temp = context.Colors.FirstOrDefault(c => c.Name == colorName.ToUpper());
                    if (temp != null)
                    {
                        return temp.Id;
                    }
                }
                return null;
            }
        }

        public Size GetSize(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Sizes.FirstOrDefault(c => c.Id == id);
            }
        }

        public Size GetSize(string name)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Sizes.FirstOrDefault(c => c.Name == name.ToUpper());
            }
        }

        public int? GetSizeId(string sizeName)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                if (sizeName != null)
                {
                    var temp = context.Sizes.FirstOrDefault(c => c.Name == sizeName.ToUpper());
                    if (temp != null)
                    {
                        return temp.Id;
                    }
                }
                return null;
            }
        }

        public MarkerCategory GetMarkerCategory(string name)
        {
            name = name.ToString();
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var marker = context.MarkerCategories.FirstOrDefault(c => c.Name == name);
                return marker;
            }
        }

        public Material GetMaterial(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Materials.FirstOrDefault(c => c.Id == id);
            }
        }

        public Material GetMaterial(string name)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Materials.FirstOrDefault(c => c.Name == name.ToUpper());
            }
        }

        public int? GetMaterialId(string materialName)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                if (materialName != null)
                {
                    var temp = context.Materials.FirstOrDefault(c => c.Name == materialName.ToUpper());
                    if (temp != null)
                    {
                        return temp.Id;
                    }
                }
                return null;
            }
        }

        public Item GetItem(Item itemWithAttributes)
        {
            Item theItem;
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<Item>(i => i.BodyStyle);
                loadOptions.LoadWith<Item>(i => i.Material);
                loadOptions.LoadWith<Item>(i => i.Color);
                loadOptions.LoadWith<Item>(i => i.Department);
                loadOptions.LoadWith<Item>(i => i.Size);
                loadOptions.LoadWith<Item>(i => i.Sleeve);
                context.LoadOptions = loadOptions;
                theItem = context.Items.FirstOrDefault(i => i.SleeveId == itemWithAttributes.SleeveId && i.BodyStyleId == itemWithAttributes.BodyStyleId && i.DepartmentId == itemWithAttributes.DepartmentId && i.MaterialId == itemWithAttributes.MaterialId && i.ColorId == itemWithAttributes.ColorId && i.SizeId == itemWithAttributes.SizeId);
                return theItem;
            }
        }

        public IEnumerable<MarkerCategory> GetMarkerCatergorys()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.MarkerCategories.ToList();
            }
        }

        public IEnumerable<MarkerDetail> GetMarkerDetails(int markerId)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<MarkerDetail>(mc => mc.Size);
                context.LoadOptions = loadOptions;
                return context.MarkerDetails.Where(md => md.MarkerCatId == markerId).ToList();
            }
        }

        public int? GetColorDetailsId(string name)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                if (name != null)
                {
                    var temp = context.ColorDetails.FirstOrDefault(c => c.Name == name.ToUpper());
                    if (temp != null)
                    {
                        return temp.ColorId;
                    }
                }
                return null;
            }
        }

        public void AddProduction (Production production)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.Productions.InsertOnSubmit(production);
                context.SubmitChanges();
            }
        }
        
        public void AddCuttingTicket(CuttingInstruction instruction)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.CuttingInstructions.InsertOnSubmit(instruction);
                context.SubmitChanges();
            }
        }

        public void AddCTSizes(IEnumerable<CuttingInstructionSize> sizes)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.CuttingInstructionSizes.InsertAllOnSubmit(sizes);
                context.SubmitChanges();
            }
        }

        public void AddCTDetails(IEnumerable<CuttingInstructionDetail> instructionDetails)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.CuttingInstructionDetails.InsertAllOnSubmit(instructionDetails);
                context.SubmitChanges();
            }
        }

        public void AddItemsRecived(IEnumerable<ReceivingItemsTransaction> items)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.ReceivingItemsTransactions.InsertAllOnSubmit(items);
                context.SubmitChanges();
            }
        }

        public CuttingInstruction GetInstruction(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<CuttingInstruction>(p => p.CuttingInstructionDetails);
                loadOptions.LoadWith<CuttingInstruction>(p => p.ReceivingItemsTransactions);
                loadOptions.LoadWith<ReceivingItemsTransaction>(r => r.Item);
                loadOptions.LoadWith<CuttingInstructionDetail>(pd => pd.Item);
                context.LoadOptions = loadOptions;
                return context.CuttingInstructions.FirstOrDefault(ct => ct.Id == id);
            }
        }

        public IEnumerable<CuttingInstruction> GetInstructions()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<CuttingInstruction>(p => p.CuttingInstructionDetails);
                loadOptions.LoadWith<CuttingInstruction>(p => p.ReceivingItemsTransactions);
                context.LoadOptions = loadOptions;
                return context.CuttingInstructions.ToList();
            }
        }

        public IEnumerable<Production> GetProductions()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<CuttingInstruction>(p => p.CuttingInstructionDetails);
                loadOptions.LoadWith<CuttingInstruction>(p => p.ReceivingItemsTransactions);
                loadOptions.LoadWith<Production>(p => p.CuttingInstructions);
                context.LoadOptions = loadOptions;
                return context.Productions.ToList();
            }
        }

        public Production GetProduction(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<CuttingInstruction>(p => p.CuttingInstructionDetails);
                loadOptions.LoadWith<CuttingInstruction>(p => p.ReceivingItemsTransactions);
                loadOptions.LoadWith<Production>(p => p.CuttingInstructions);
                loadOptions.LoadWith<ReceivingItemsTransaction>(r => r.Item);
                loadOptions.LoadWith<CuttingInstructionDetail>(pd => pd.Item);
                context.LoadOptions = loadOptions;
                return context.Productions.FirstOrDefault(p => p.Id == id);
            }
        }

        public Production GetProductionForExcel(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<CuttingInstruction>(p => p.CuttingInstructionDetails);
                loadOptions.LoadWith<CuttingInstruction>(p => p.MarkerCategory);
                loadOptions.LoadWith<MarkerCategory>(p => p.MarkerDetails);
                loadOptions.LoadWith<CuttingInstruction>(p => p.CuttingInstructionSizes);
                loadOptions.LoadWith<Production>(p => p.CuttingInstructions);
                loadOptions.LoadWith<CuttingInstructionDetail>(pd => pd.Item);
                loadOptions.LoadWith<Item>(pd => pd.Color);
                loadOptions.LoadWith<Item>(pd => pd.Material);
                context.LoadOptions = loadOptions;
                return context.Productions.FirstOrDefault(p => p.Id == id);
            }
        }

        public IEnumerable<CuttingInstruction> GetOpenedInstructions()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<CuttingInstruction>(p => p.CuttingInstructionDetails);
                loadOptions.LoadWith<CuttingInstruction>(p => p.Production);
                loadOptions.LoadWith<CuttingInstruction>(p => p.ReceivingItemsTransactions);
                context.LoadOptions = loadOptions;
                return context.CuttingInstructions.Where(i =>
                (i.CuttingInstructionDetails.Count() > 0 ? i.CuttingInstructionDetails.Sum(d => d.Quantity): 0)
                != (i.ReceivingItemsTransactions.Count() > 0 ? i.ReceivingItemsTransactions.Sum(d => d.Quantity) : 0)).ToList();
                
            }
        }
        public IEnumerable<Production> GetOpenedProductions()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<CuttingInstruction>(p => p.CuttingInstructionDetails);
                loadOptions.LoadWith<CuttingInstruction>(p => p.ReceivingItemsTransactions);
                loadOptions.LoadWith<Production>(p => p.CuttingInstructions);
                context.LoadOptions = loadOptions;
                var prods = new List<Production>();
                foreach(var productin in context.Productions)
                {
                    if(productin.CuttingInstructions.Count() > 0)
                    {
                        var cuttingInstructionDetailsSum = productin.CuttingInstructions.Where(c => c.CuttingInstructionDetails.Count() > 0).Sum(c => c.CuttingInstructionDetails.Sum(cd => cd.Quantity));
                        var recivedItemSum = productin.CuttingInstructions.Where(c => c.ReceivingItemsTransactions.Count() > 0).Sum(c => c.ReceivingItemsTransactions.Sum(cd => cd.Quantity));
                        if(cuttingInstructionDetailsSum != recivedItemSum)
                        {
                            prods.Add(productin);
                        }
                    }
                }
                return prods.ToList();
            }
        }

        public IEnumerable<int> GetOpenedInstructionsIds()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<CuttingInstruction>(p => p.CuttingInstructionDetails);
                loadOptions.LoadWith<CuttingInstruction>(p => p.ReceivingItemsTransactions);
                context.LoadOptions = loadOptions;
                return context.CuttingInstructions.Where(i =>
                (i.CuttingInstructionDetails.Count() > 0 ? i.CuttingInstructionDetails.Sum(d => d.Quantity) : 0)
                != (i.ReceivingItemsTransactions.Count() > 0 ? i.ReceivingItemsTransactions.Sum(d => d.Quantity) : 0)).Select(ct => ct.Id).ToList();
            }
        }

        public CuttingInstructionDetail GetInstructionDetail(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.CuttingInstructionDetails.FirstOrDefault(d => d.Id == id);
            }
        }

        public void UpdateCID(int Id, int quantity)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.ExecuteCommand("UPDATE CuttingInstructionDetails SET Quantity = {0} WHERE Id = {1}", quantity, Id);
            }
        } 
    }
}

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
        
        public void AddCuttingTicket(CuttingInstruction instruction)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.CuttingInstructions.InsertOnSubmit(instruction);
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
    }
}

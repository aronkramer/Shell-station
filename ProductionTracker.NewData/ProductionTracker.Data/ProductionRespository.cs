﻿using System;
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

        public int LastLotNumber()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.CuttingInstructions.Count() > 1 ? context.CuttingInstructions.OrderByDescending(c => c.LotNumber).First().LotNumber : 2222;
            }
        }

        public Department GetDepartment(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Departments.FirstOrDefault(c => c.Id == id);
            }
        }

        public BodyStyle GetBodyStyle(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.BodyStyles.FirstOrDefault(c => c.Id == id);
            }
        }

        public Sleeve GetSleeve (int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Sleeves.FirstOrDefault(c => c.Id == id);
            }
        }

        public Color GetColor(int id)
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

        public IEnumerable<Color> GetColors()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Colors.ToList();
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

        public IEnumerable<ColorDetail> GetColorDetails()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.ColorDetails.ToList();
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

        public IEnumerable<Size> GetSizes()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Sizes.ToList();
            }
        }

        public Fabric GetFabric(Fabric fabric)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Fabrics.FirstOrDefault(f => f.MaterialId == fabric.MaterialId && f.ColorId == fabric.ColorId);
            }
        }
        public void AddFabric(Fabric fabric)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.Fabrics.InsertOnSubmit(fabric);
                context.SubmitChanges();
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

        public IEnumerable<MarkerCategory> GetMarkerCatergorys()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.MarkerCategories.ToList();
            }
        }

        public Marker GetMarker(int catId, List<MarkerDetail> Sizes)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var markers = context.Markers.Where(m => m.MarkerCatId == catId).Where(m => m.MarkerDetails.Count() == Sizes.Count()).ToList();
                var marker =  markers.FirstOrDefault(m => m.MarkerDetails.All(x => Sizes.Any(y => x.SizeId == y.SizeId && x.AmountPerLayer == y.AmountPerLayer)));
                return marker;

            }
        } 

        public void AddMarker(Marker marker)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.Markers.InsertOnSubmit(marker);
                context.SubmitChanges();
            }
        }

        public void AddMarkerDetails(IEnumerable<MarkerDetail> markerDetails)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.MarkerDetails.InsertAllOnSubmit(markerDetails);
                context.SubmitChanges();
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

        public IEnumerable<Material> GetMaterials()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Materials.ToList();
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

        public IEnumerable<MarkerDetail> GetMarkerDetails(int markerCatId)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<MarkerDetail>(mc => mc.Size);
                context.LoadOptions = loadOptions;
                var markerId = context.MarkerCategories.FirstOrDefault(mc => mc.Id == markerCatId).DefaltMarkerId;
                return context.MarkerDetails.Where(md => md.MarkerId == markerId).ToList();
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

        public void AddCTItems(IEnumerable<CuttingInstructionItem> instructionDetails)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.CuttingInstructionItems.InsertAllOnSubmit(instructionDetails);
                context.SubmitChanges();
            }
        }

        public void AddCTDetail(CuttingInstructionDetail instructionDetail)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.CuttingInstructionDetails.InsertOnSubmit(instructionDetail);
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
                loadOptions.LoadWith<CuttingInstructionDetail>(p => p.CuttingInstructionItems);
                loadOptions.LoadWith<CuttingInstruction>(p => p.ReceivingItemsTransactions);
                loadOptions.LoadWith<ReceivingItemsTransaction>(r => r.Item);
                loadOptions.LoadWith<CuttingInstructionItem>(pd => pd.Item);
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
                loadOptions.LoadWith<CuttingInstructionDetail>(p => p.CuttingInstructionItems);
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
                loadOptions.LoadWith<CuttingInstructionDetail>(p => p.CuttingInstructionItems);
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
                loadOptions.LoadWith<CuttingInstructionDetail>(p => p.CuttingInstructionItems);
                loadOptions.LoadWith<CuttingInstruction>(p => p.ReceivingItemsTransactions);
                loadOptions.LoadWith<Production>(p => p.CuttingInstructions);
                loadOptions.LoadWith<ReceivingItemsTransaction>(r => r.Item);
                loadOptions.LoadWith<CuttingInstructionItem>(pd => pd.Item);
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
                loadOptions.LoadWith<CuttingInstructionDetail>(p => p.Fabric);
                loadOptions.LoadWith<Fabric>(p => p.Color);
                loadOptions.LoadWith<Fabric>(p => p.Material);
                loadOptions.LoadWith<Production>(p => p.CuttingInstructions);
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
                loadOptions.LoadWith<CuttingInstructionDetail>(p => p.CuttingInstructionItems);
                loadOptions.LoadWith<CuttingInstruction>(p => p.Production);
                loadOptions.LoadWith<CuttingInstruction>(p => p.ReceivingItemsTransactions);
                context.LoadOptions = loadOptions;
                return context.CuttingInstructions.Where(i =>
                (i.CuttingInstructionDetails.Count() > 0 ? i.CuttingInstructionDetails.Sum(co => co.CuttingInstructionItems.Sum(d => d.Quantity)) : 0)
                != (i.ReceivingItemsTransactions.Count() > 0 ? i.ReceivingItemsTransactions.Sum(d => d.Quantity) : 0)).ToList();
                
                
            }
        }
        public IEnumerable<Production> GetOpenedProductions()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                //loadOptions.LoadWith<CuttingInstruction>(p => p.CuttingInstructionItems);
                loadOptions.LoadWith<CuttingInstruction>(p => p.ReceivingItemsTransactions);
                loadOptions.LoadWith<Production>(p => p.CuttingInstructions);
                context.LoadOptions = loadOptions;
                var prods = new List<Production>();
                foreach(var productin in context.Productions)
                {
                    if(productin.CuttingInstructions.Count() > 0)
                    {
                        var CuttingInstructionItemsSum = productin.CuttingInstructions.Where(c => c.CuttingInstructionDetails.Count() > 0).Sum(c => c.CuttingInstructionDetails.Sum(cd => cd.CuttingInstructionItems.Sum(d => d.Quantity)));
                        var recivedItemSum = productin.CuttingInstructions.Where(c => c.ReceivingItemsTransactions.Count() > 0).Sum(c => c.ReceivingItemsTransactions.Sum(cd => cd.Quantity));
                        if (CuttingInstructionItemsSum != recivedItemSum)
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
                //var loadOptions = new DataLoadOptions();
                //loadOptions.LoadWith<CuttingInstruction>(p => p.CuttingInstructionItems);
                //loadOptions.LoadWith<CuttingInstruction>(p => p.ReceivingItemsTransactions);
                //context.LoadOptions = loadOptions;
                //return context.CuttingInstructions.Where(i =>
                //(i.CuttingInstructionItems.Count() > 0 ? i.CuttingInstructionItems.Sum(d => d.Quantity) : 0)
                //!= (i.ReceivingItemsTransactions.Count() > 0 ? i.ReceivingItemsTransactions.Sum(d => d.Quantity) : 0)).Select(ct => ct.Id).ToList();
                return null;
            }
        }

        public CuttingInstructionItem GetInstructionDetail(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.CuttingInstructionItems.FirstOrDefault(d => d.Id == id);
            }
        }

        public void UpdateCID(int Id, int quantity)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.ExecuteCommand("UPDATE CuttingInstructionItems SET Quantity = {0} WHERE Id = {1}", quantity, Id);
            }
        } 
    }
}

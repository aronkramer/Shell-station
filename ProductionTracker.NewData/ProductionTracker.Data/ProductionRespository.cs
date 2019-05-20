using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProductionTracker.Data.Models;

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

        public int LotNumberIndex()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Settings.Select(s => s.LotNumberCounter).FirstOrDefault();
            }
        }

        public void IncrementLotNumberCounter(int amount)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var setting = context.Settings.FirstOrDefault();
                setting.LotNumberCounter += amount;
                context.Refresh(RefreshMode.KeepCurrentValues, setting);
                context.SubmitChanges();
            }
        }

        public Department GetDepartment(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Departments.FirstOrDefault(c => c.Id == id);
            }
        }

        public IEnumerable< Department> GetDepartments()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Departments.ToList();
            }
        }

        public BodyStyle GetBodyStyle(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.BodyStyles.FirstOrDefault(c => c.Id == id);
            }
        }

        public IEnumerable<BodyStyle> GetBodyStyles()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.BodyStyles.ToList();
            }
        }

        public Sleeve GetSleeve (int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Sleeves.FirstOrDefault(c => c.Id == id);
            }
        }

        public IEnumerable<Sleeve> GetSleeves()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Sleeves.ToList();
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
        public IEnumerable<Fabric> GetFabicsWithColorDes()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Fabrics.Where(f=> f.ColorDescriptionName != null).ToList();
            }
        }

        public MarkerCategory GetMarkerCategory(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.MarkerCategories.FirstOrDefault(c => c.Id == id);
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

        public IEnumerable<MarkerDetail> GetDefaltMarkerDetails(string name)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOption = new DataLoadOptions();
                loadOption.LoadWith<MarkerDetail>(c => c.Size);
                context.LoadOptions = loadOption;
                var defaltId = context.MarkerCategories.FirstOrDefault(c => c.Name == name).DefaltMarkerId;
                return context.MarkerDetails.Where(m => m.MarkerId == defaltId).ToList();
            }
        }
        public IEnumerable<MarkerDetail> GetDefaltMarkerDetails(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOption = new DataLoadOptions();
                loadOption.LoadWith<MarkerDetail>(c => c.Size);
                context.LoadOptions = loadOption;
                var defaltId = context.MarkerCategories.FirstOrDefault(c => c.Id == id).DefaltMarkerId;
                return context.MarkerDetails.Where(m => m.MarkerId == defaltId).ToList();
            }
        }

        public IEnumerable<MarkerDetail> GetMarkerDetails(int markerId)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {

                return context.MarkerDetails.Where(m => m.MarkerId == markerId).ToList();
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

        public IEnumerable<MarkerDetail> GetMarkerDetailsFromCat(int markerCatId)
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

        public IEnumerable<ProductionCatergory> GetProductionCatergories()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.ProductionCatergories.ToList();
            }
        }

        public ProductionCatergory GetProductionCatergory(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.ProductionCatergories.FirstOrDefault(pc => pc.Id == id);
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

        public IEnumerable<CuttingInstruction> GetNotCompleteInstructionForLots()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
               
                return context.CuttingInstructions.Where(ct => !ct.Completed).ToList();
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

        public ReceivingItemsTransaction GetReceivingItemsTransaction(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.ReceivingItemsTransactions.FirstOrDefault(r => r.Id == id);
            }
        }

        //PLANNED PRODUCTIONS

        public void AddPlannedProduction(PlannedProduction plannedProduction)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.PlannedProductions.InsertOnSubmit(plannedProduction);
                context.SubmitChanges();
            }
        }

        public void AddPlannedProductionDetails(IEnumerable<PlannedProductionDetail> plannedProductionDetails)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.PlannedProductionDetails.InsertAllOnSubmit(plannedProductionDetails);
                context.SubmitChanges();
            }
        }

        public IEnumerable<PlannedProduction> GetPlannedProductions()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                
                loadOptions.LoadWith<PlannedProduction>(pd => pd.ProductionCatergory);
                context.LoadOptions = loadOptions;
                return context.PlannedProductions.Where(x => !x.Deleted).ToList();
            }
        }

        public PlannedProduction GetPlannedProduction(PlannedProduction plannedProduction)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.PlannedProductions.Where(x => !x.Deleted).FirstOrDefault(p => p.ProductionCatergoryId == plannedProduction.ProductionCatergoryId && p.ProductionCatYear == plannedProduction.ProductionCatYear);
            }

        }

        public PlannedProduction GetPlannedProduction(int plannedProductionId)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.PlannedProductions.Where(x => !x.Deleted).FirstOrDefault(p => p.Id == plannedProductionId);
            }

        }

        public PlannedProduction GetPlannedProductionWithDetails(PlannedProduction plannedProduction)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOption = new DataLoadOptions();
                loadOption.LoadWith<PlannedProduction>(p => p.PlannedProductionDetails);
                loadOption.LoadWith<PlannedProductionDetail>(p => p.Item);
                context.LoadOptions = loadOption;
                
                var result =  context.PlannedProductions.NotDeleted().FirstOrDefault(p => p.ProductionCatergoryId == plannedProduction.ProductionCatergoryId && p.ProductionCatYear == plannedProduction.ProductionCatYear);
                if(result.NotNull())
                result.PlannedProductionDetails = result.PlannedProductionDetails.Where(x => !x.Deleted).ToEntitySet();
                return result;
            }

        }

        public IEnumerable<PlannedProductionDetail> GetPlannedProductionDetails(int plannedProductionId)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<PlannedProductionDetail>(p => p.Item);
                context.LoadOptions = loadOptions;
                return context.PlannedProductionDetails.Where(x => x.Deleted).Where(pd => pd.PlannedProductionId == plannedProductionId).ToList();
            }
        }

        public PlannedProductionDetail GetPlannedProductionDetail(int plannedProductionDetailId)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.PlannedProductionDetails.Where(x => !x.Deleted).FirstOrDefault(p => p.Id == plannedProductionDetailId);
            }
        }

        public void UpdatePlannedProductionDetail(PlannedProductionDetail plannedProductionDetail)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                plannedProductionDetail.ModifiedOn = DateTime.Now;
                context.PlannedProductionDetails.Attach(plannedProductionDetail);
                context.Refresh(RefreshMode.KeepCurrentValues, plannedProductionDetail);
                context.SubmitChanges();
            }
        }

        public void UpdatePlannedProductionDetails(IEnumerable<PlannedProductionDetail> plannedProductionDetails)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.PlannedProductionDetails.AttachAll(plannedProductionDetails);
                context.Refresh(RefreshMode.KeepCurrentValues, plannedProductionDetails);
                context.SubmitChanges();
            }
        }

        public void DeletePlannedProductionDetail(PlannedProductionDetail plannedProductionDetail)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                plannedProductionDetail.Deleted = true;
                //context.PlannedProductionDetails.Attach(plannedProductionDetail);
                context.Refresh(RefreshMode.KeepCurrentValues, plannedProductionDetail);
                context.SubmitChanges();
            }
        }

        public void DeletePlannedProduction(PlannedProduction plannedProduction)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                plannedProduction.Deleted = true;
                context.PlannedProductions.Attach(plannedProduction);
                context.Refresh(RefreshMode.KeepCurrentValues, plannedProduction);
                context.SubmitChanges();
            }
        }

        public void DeletePlannedProductionDetail(int plannedProductionDetailId)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var plannedProductionDetail = context.PlannedProductionDetails.FirstOrDefault(p => p.Id == plannedProductionDetailId);
                plannedProductionDetail.Deleted = true;
                //context.PlannedProductionDetails.Attach(plannedProductionDetail);
                context.Refresh(RefreshMode.KeepCurrentValues, plannedProductionDetail);
                context.SubmitChanges();
            }
        }

        public void DeletePlannedProduction(int plannedProductionId)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var plannedProduction = context.PlannedProductions.FirstOrDefault(p => p.Id == plannedProductionId);
                //if (plannedProduction == null)
                //    throw new Exception("The id does not exist");
                plannedProduction.Deleted = true;
                context.PlannedProductions.Attach(plannedProduction);
                context.Refresh(RefreshMode.KeepCurrentValues, plannedProduction);
                context.SubmitChanges();
            }
        }

        public IEnumerable<PlannedProduction> GetPlannedProductionsForItems()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var pp = context.PlannedProductions.FirstOrDefault();
                //pp.CuttingInstructions.FirstOrDefault().ReceivingItemsTransactions CuttingInstructionDetails.FirstOrDefault().CuttingInstructionItems
                return context.PlannedProductions.OrderByDescending(s => s.CreatedOn).Take(2).ToList();
            }
        }


        //public void DeletePlannedProduction(int plannedProductioId)
        //{
        //    using (var context = new ManufacturingDataContext(_connectionString))
        //    {
        //        context.ExecuteCommand("DELETE FROM PlannedProductions WHERE Id = {0}", plannedProductioId);

        //    }
        //}

        //public void DeletePlannedProductionDetail(int plannedProductionDetailId)
        //{
        //    using (var context = new ManufacturingDataContext(_connectionString))
        //    {
        //        context.ExecuteCommand("DELETE FROM PlannedProductionDetails WHERE Id = {0}", plannedProductionDetailId);
        //    }
        //}

        public IEnumerable<CuttingInstruction> GetNonCompleteInstructions()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<CuttingInstruction>(p => p.CuttingInstructionDetails);
                loadOptions.LoadWith<CuttingInstructionDetail>(p => p.CuttingInstructionItems);
                loadOptions.LoadWith<CuttingInstruction>(p => p.Production);
                loadOptions.LoadWith<CuttingInstruction>(p => p.ReceivingItemsTransactions);
                context.LoadOptions = loadOptions;
                return context.CuttingInstructions.Where(i => i.Completed == false).ToList();
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
                (i.CuttingInstructionDetails.NotNUllOrEmpty() ? i.CuttingInstructionDetails.Sum(co => co.CuttingInstructionItems.Sum(d => d.Quantity)) : 0)
                != (i.ReceivingItemsTransactions.NotNUllOrEmpty() ? i.ReceivingItemsTransactions.Sum(d => d.Quantity) : 0)).ToList();
                
                
            }
        }

        public IEnumerable<int> GetClosedInstructionsIds()
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
                <= (i.ReceivingItemsTransactions.Count() > 0 ? i.ReceivingItemsTransactions.Sum(d => d.Quantity) : 0)).Select(ct => ct.Id).ToList();


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

        public IEnumerable<Production> GetNonClosedProductions()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                //loadOptions.LoadWith<CuttingInstruction>(p => p.CuttingInstructionItems);
                loadOptions.LoadWith<CuttingInstruction>(p => p.ReceivingItemsTransactions);
                loadOptions.LoadWith<Production>(p => p.CuttingInstructions);
                context.LoadOptions = loadOptions;
                var prods = new List<Production>();
                foreach (var productin in context.Productions.Where(p => !p.CuttingInstructions.All(i => i.Completed == true)))
                {
                    if (productin.CuttingInstructions.Count() > 0)
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

        public void MarkCuttingTicketAsCompleate(int id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.ExecuteCommand("UPDATE CuttingInstructions SET Completed = 1 WHERE Id = {0}", id);
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

        //UPDATE HISTORY

        public IEnumerable<UpdateHistory> GetUpdateHistories()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.UpdateHistories.ToList();
            }
        }

        public void AddNewUpdateHistory (dynamic obj, string action = "updated")
        {
            var updateHistory = new UpdateHistory
            {
                Action = action,
                PropertyId = obj.Id,
                PropertyType = obj.GetType().Name,
                OldObjectData = Helpers.GetBasePropertiesOnDbObject(obj)
            };
            AddNewUpdateHistory(updateHistory);
        }
        public void AddNewUpdateHistory (UpdateHistory updateHistory)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.UpdateHistories.InsertOnSubmit(updateHistory);
                context.SubmitChanges();
            }
        }

        public dynamic GetHistorysOfOneObject (dynamic obj)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var x = context.UpdateHistories.ToList().Where(h => h.PropertyType == obj.GetType().Name && h.PropertyId == obj.Id).Select(h => {
                    dynamic re = JsonConvert.DeserializeObject(h.OldObjectData, obj.GetType());
                    re.ModifiedOn = h.CreatedOn;
                    return re;
                });
                return x;
            }
        }

        
    }
}

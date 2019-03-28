using System.Collections.Generic;
using System.Linq;
using BarcodeGenatraterCR;

using ProductionTracker.Data;
using ProductionTracker.Web.Models;

namespace ProductionTracker.Web.Reports
{
    public class ReportController
    {
        //NORTHWNDEntities db = new NORTHWNDEntities();
        ItemRepository db = new ItemRepository(Properties.Settings.Default.ManufacturingConStr);
        //public ReportController() { }

        public List<TestLable> ProjectnWiseLandStatementSummary()  
        {
            var data = (from a in db.GetItemsForBarcodes()
                        select new TestLable
                        {
                            Id = (int)a.Id,
                            SKU = a.SKU,
                            Sleeve = a.Sleeve.ToString(),
                            Barcode = $"**{a.Id}*"
                        }).ToList();
            return data;
        }

        public List<BarcodeLable> RealBarcodes()
        {
            var data = (from a in db.GetItemsForBarcodes()
                        select new BarcodeLable
                        {
                            Id = (int)a.Id,
                            Sleeve = a.Sleeve,
                            Material = a.Material,
                            BodyStyle = a.BodyStyle,
                            BodyStyleAtt = a.BodyStyleAtt,
                            Color = a.Color,
                            ColorId = (int)a.ColorId,
                            Departmet = a.Departmet,
                            Size = a.Size,
                            SizeDis = a.SizeDis,
                            SKU = a.SKU,
                            MaterialId = (int)a.MaterialId
                        }).ToList();
            return data;
        }
        public List<BarcodeLable> ConvertProductionIntoBarcodeItems(IEnumerable<CuttingInstructionItem> cuttingInstructionItems, int amountPerSheet)
        {
            var repo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var barcodeItems = RealBarcodes();
            var items = new List<BarcodeLable>();
            var fabricsWithDis = repo.GetFabicsWithColorDes();
            cuttingInstructionItems.ToList().ForEach(i =>
            {
                var tempItem = barcodeItems.FirstOrDefault(bi => bi.Id == i.ItemId);
                if (tempItem != null)
                {
                    var colordis = fabricsWithDis.FirstOrDefault(f => f.MaterialId == tempItem.MaterialId && f.ColorId == tempItem.ColorId);
                    if(colordis != null)
                    {
                        tempItem.Color = colordis.ColorDescriptionName;
                    }
                    var mod = i.Quantity % amountPerSheet;
                    var result = mod != 0 ? (i.Quantity - mod) + amountPerSheet : i.Quantity;
                    items.AddRange(Enumerable.Repeat(tempItem, result));

                }

            });
            return items;
        }
    }
}


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
                            SKU = a.SKU
                        }).ToList();
            return data;
        }
        public List<BarcodeLable> ConvertProductionIntoBarcodeItems(IEnumerable<CuttingInstructionItem> cuttingInstructionItems, int amountPerSheet)
        {
            var barcodeItems = RealBarcodes();
            var items = new List<BarcodeLable>();
            cuttingInstructionItems.ToList().ForEach(i =>
            {
                var tempItem = barcodeItems.FirstOrDefault(bi => bi.Id == i.ItemId);
                if (tempItem != null)
                {
                    var mod = i.Quantity % amountPerSheet;
                    var result = mod != 0 ? (i.Quantity - mod) + 80 : i.Quantity;
                    items.AddRange(Enumerable.Repeat(tempItem, result));

                }

            });
            return items;
        }
    }
}


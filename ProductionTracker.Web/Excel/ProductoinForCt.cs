using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProductionTracker.Data;

namespace ProductionTracker.Web.Excel
{
    public class MarkerWithColorMaterials
    {
        public MarkerWithColorMaterials()
        {
            ColorMaterials = new List<ColorMaterial>();
            Sizes = new List<SizeWithLayer>();
        }
        public string Name { get; set; }
        public string Size { get; set; }
        public List<SizeWithLayer> Sizes { get; set; }
        public List<ColorMaterial> ColorMaterials { get; set; }
        public int LotNumber { get; set; }
    }
    public class ProductionForCT
    {
        public ProductionForCT()
        {
            Markers = new List<MarkerWithColorMaterials>();
        }
        public string Name { get; set; }
        public List<MarkerWithColorMaterials> Markers { get; set; }
    }
    public class ColorMaterial
    {
        public string Color { get; set; }
        public string Material { get; set; }
        public int Layers { get; set; }
    }
    public class SizeWithLayer
    {
        public int SizeId { get; set; }
        public int AmountPerLayer { get; set; }

    }
    public class ErrorsAndItems
    {
        public DateTime Date { get; set; }
        public List<CuttingInstructionDetail> Items { get; set; }
        public List<string> Errors { get; set; }

    }
}
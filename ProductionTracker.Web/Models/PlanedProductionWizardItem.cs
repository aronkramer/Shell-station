using ProductionTracker.Data;
namespace ProductionTracker.Web.Models
{
    public class PlanedProductionWizardItem
    {
        public int ColorId { get; set; }
        public int SizeId { get; set; }
        public int MaterialId { get; set; }
        public int Quantity { get; set; }
    }
    public class FinalItem
    {
        public Item Item { get; set; }
        public int Quantity { get; set; }
    }
}
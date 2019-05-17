using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionTracker.Data.Models
{
    public class ItemWithActivity
    {
        public ItemWithActivity()
        {
            Activities = new List<ItemActivity>();
        }
        public Item Item { get; set; }
        public IEnumerable<ItemActivity> Activities { get; set; }
    }

    public class ItemActivity
    {
        public int Id { get; set; }
        public ActivityType Type { get; set; }
        public DateTime Date { get; set; }
        public string DatePretty { get; set; }
        public int Quantity { get; set; }
        public int? CuttingInstructionId { get; set; }
        public Season Season { get; set; }
    }
    public enum ActivityType
    {
        Ordered,
        Received
    }
    public class Season
    {
        public int? PlannedProductionId { get; set; }
        public string Name { get; set; }
    }
}

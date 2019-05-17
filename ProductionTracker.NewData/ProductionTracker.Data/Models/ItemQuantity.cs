using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionTracker.Data.Models
{
    public class ItemQuantity
    {
        public int AmountOrdered { get; set; }
        public int AmountReceived { get; set; }
        public int PlannedAmount { get; set; }
    }
    public class ItemWithQuantity
    {
        public Item Item { get; set; }
        public ItemQuantity Quantitys { get; set; }
        public string LastCuttingInstructionDatePretty { get; set; }
        public DateTime LastCuttingInstructionDate { get; set; }
    }
    public class SeasonWithItems
    {
        public Season Season { get; set; }
        public IEnumerable<ItemWithQuantity> ItemsWithQuantities { get; set; }
    }
    
}

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
    }
    public class ItemWithQuantity
    {
        public Item Item { get; set; }
        public ItemQuantity Quantitys { get; set; }
        public string LastCuttingInstructionDate { get; set; }
    }
}

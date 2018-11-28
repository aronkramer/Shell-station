using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProductionTracker.OldData;

namespace ProductionTracker.Web.Models
{
    public class ItemWithQuantityVM
    {
        public Item Item { get; set; }
        public ItemQuantity Quantitys { get; set; }
        public DateTime LastProductionDate { get; set; }
    }
}
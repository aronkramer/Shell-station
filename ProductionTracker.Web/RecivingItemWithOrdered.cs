using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionTracker.Web
{
    public class RecivingItemWithOrdered
    {
        public int ItemId { get; set; }
        public int CuttingInstuctionId { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
        public int OrderedId { get; set; }
    }
}
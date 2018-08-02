using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionTracker.Web.Models
{
    public class ItemActivity
    {
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
    }
}
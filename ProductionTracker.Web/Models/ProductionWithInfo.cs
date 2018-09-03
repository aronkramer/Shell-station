using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionTracker.Web.Models
{
    public class ProductionWithInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ItemAmount { get; set; }
        public string Date { get; set; }
        public string PercentageFilled { get; set; }
    }
}
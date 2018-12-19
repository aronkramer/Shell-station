using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionTracker.Web.Models
{
    public class CutingIntructionDetailsVM
    {
        public int Id { get; set; }
        public string SKU { get; set; }
        public int OrderedId { get; set; }
        public int Ordered { get; set; }
        public int Received { get; set; }
        public string PercentageFilled { get; set; }
        public int Lot { get; set; }
        public int CuttingInstructionId { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionTracker.Web.Models
{
    public class ItemActivity
    {
        public int Id { get; set; }
        public ActivityType Type { get; set; }
        public string Date { get; set; }
        public int Quantity { get; set; }
        public int? CuttingInstructionId { get; set; }
    }
    public enum ActivityType
    {
        Ordered,
        Received
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionTracker.Data
{
    public class CuttingInstructionItemWithQuantityRecived
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string SKU { get; set; }
        public int QuantityOrdered { get; set; }
        public int QuantityReceived { get; set; }
        public int CuttingInstructionId { get; set; }
        public int LotNumber { get; set; }

    }
}

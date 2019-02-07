using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AmarMati.Reports.ReportModel
{
    public class SummeryModel
    {
        public SummeryModel() { }

        public String ProjectName { get; set; }
        public String ProjectShortName { get; set; }
        public String OwnershipTypeName { get; set; }
        public String CategoryName { get; set; }
        public Decimal? LandQtyInDecimal { get; set; }
        public Decimal? LandQtyInSquareMeter { get; set; }
        public Decimal? LandQtyInBigha { get; set; }
        public Decimal? LandQtyInAcre { get; set; }
        public Decimal? PurchasedLandQty { get; set; }
        public Decimal? MutationLandQty { get; set; }
    }
}
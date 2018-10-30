using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProductionTracker.Data;


namespace ProductionTracker.Web.Models
{
    public class NewMarkerVM
    {
        public IEnumerable<Department> Departments { get; set; }
        public IEnumerable<BodyStyle> Styles { get; set; }
        public IEnumerable<Sleeve> Sleeves { get; set; }
    }
}
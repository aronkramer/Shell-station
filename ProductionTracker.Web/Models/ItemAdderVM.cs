using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProductionTracker.OldData;

namespace ProductionTracker.Web.Models
{
    public class ItemAdderVM
    {
        public IEnumerable<Department> Departments { get; set; }
        public IEnumerable<Material> Materials { get; set; }
        public IEnumerable<Color> Colors { get; set; }
        public IEnumerable<BodyStyle> Styles { get; set; }
        public IEnumerable<Sleeve> Sleeves { get; set; }
        public List<string> CheckedDepartments { get; set; }
    }
}
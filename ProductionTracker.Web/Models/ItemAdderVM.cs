using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProductionTracker.Data;

namespace ProductionTracker.Web.Models
{
    public class ItemAdderVM
    {
        public IEnumerable<Department> Departments { get; set; }
        public IEnumerable<Fabric> Fabrics { get; set; }
        public IEnumerable<Color> Colors { get; set; }
        public List<string> CheckedDepartments { get; set; }
    }
}
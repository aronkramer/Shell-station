using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionTracker.OldData
{
    public class FabricRepository
    {
        private string _connectionString;
        public FabricRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public IEnumerable<Material> AllMaterial()
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                return context.Materials.ToList();
            }
        }
    }
}

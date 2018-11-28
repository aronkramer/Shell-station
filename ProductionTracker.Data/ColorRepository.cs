using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionTracker.OldData
{
    public class ColorRepository
    {
        private string _connectionString;
        public ColorRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public void AddColors(IEnumerable<Color> colors)
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                context.Colors.InsertAllOnSubmit(colors);
                context.SubmitChanges();
            }
        }
        public IEnumerable<Color> GetAllColors()
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                return context.Colors.ToList();
            }
        }
    }
}

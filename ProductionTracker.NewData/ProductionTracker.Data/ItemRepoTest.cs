using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionTracker.Data
{
    public class ItemRepoTest
    {
        private string _connectionString;

        public ItemRepoTest(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddItem(Item item)
        {
            using (var context = new ManufacturingDataContext())
            {
                context.Items.InsertOnSubmit(item);
                context.SubmitChanges();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;

namespace ProductionTracker.Data
{
    public class ProductionRepository
    {
        private string _connectionString;
        public ProductionRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        

        public IEnumerable<Production> GetAllProductions()
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<Production>(p => p.ProductionDetails);
                loadOptions.LoadWith<Production>(p => p.ReceivedItems);
                context.LoadOptions = loadOptions;
                return context.Productions.ToList();
            }
        }

        public Production GetProductionById(int id)
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<Production>(p => p.ProductionDetails);
                loadOptions.LoadWith<Production>(p => p.ReceivedItems);
                loadOptions.LoadWith<ReceivedItem>(r => r.Item);
                loadOptions.LoadWith<ProductionDetail>(pd => pd.Item);
                context.LoadOptions = loadOptions;
                return context.Productions.FirstOrDefault(i => i.Id == id);
            }
        }

        public IEnumerable<Department> GetDepartments()
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                return context.Departments.ToList();
            }
        }
        
        public IEnumerable<Material> GetAllMaterials()
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
               
                return context.Materials.ToList();
            }
        }

        public IEnumerable<Sleeve> GetAllSleeves()
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                return context.Sleeves.ToList();
            }
        }

        public IEnumerable<BodyStyle> GetAllStyles()
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                return context.BodyStyles.ToList();
            }
        }

        public IEnumerable<Size> GetAllSizesByDepartment(int depId)
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                return context.SizeDepartments.Where(sd => sd.DepartmentId == depId).Select(i => i.Size).ToList();
            }
        }
        public IEnumerable<BodyStyle> AddBodyStyles(List<BodyStyle> styles)
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                context.BodyStyles.InsertAllOnSubmit(styles);
                context.SubmitChanges();
                return styles;
            }
        }
        public IEnumerable<Sleeve> AddSleeves(List<Sleeve> sleeve)
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                context.Sleeves.InsertAllOnSubmit(sleeve);
                context.SubmitChanges();
                return sleeve;
            }
        }
        public IEnumerable<Size> AddSizes(List<Size> size)
        {
            using (var context = new ProductionDataContext(_connectionString))
            {
                context.Sizes.InsertAllOnSubmit(size);
                context.SubmitChanges();
                return size;
            }
        }

    }
}


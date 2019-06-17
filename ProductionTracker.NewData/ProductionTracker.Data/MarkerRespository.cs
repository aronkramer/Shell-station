using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionTracker.Data
{
    public class MarkerRespository
    {
        private string _connectionString;

        public MarkerRespository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<MarkerCategory> GetMarkerCategoriesWithAllMarkers()
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                var loadOptions = new DataLoadOptions();
                loadOptions.LoadWith<MarkerCategory>(m => m.Marker);
                loadOptions.LoadWith<MarkerCategory>(m => m.Department);
                loadOptions.LoadWith<MarkerCategory>(m => m.BodyStyle);
                loadOptions.LoadWith<MarkerCategory>(m => m.Sleeve);
                loadOptions.LoadWith<MarkerCategory>(m => m.Markers);
                loadOptions.LoadWith<Marker>(m => m.MarkerDetails);
                loadOptions.LoadWith<MarkerDetail>(m => m.Size);
                context.LoadOptions = loadOptions;
                return context.MarkerCategories.ToList().Select(m => {
                    m.Markers = m.Markers.Where(ma => !ma.Deleted).ToEntitySet();
                    return m;
                });
            }
        }

        public void AddMarkerCat(MarkerCategory marker)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.MarkerCategories.InsertOnSubmit(marker);
                context.SubmitChanges();
            }
        }

        public void AddMarker(Marker marker)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.Markers.InsertOnSubmit(marker);
                context.SubmitChanges();
            }
        }

        public void AddMarkerDetails(IEnumerable<MarkerDetail> markerDetails)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                context.MarkerDetails.InsertAllOnSubmit(markerDetails);
                context.SubmitChanges();
            }
        }

        public void UpdateMarkerCat(MarkerCategory markerCategory)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                markerCategory.ModifiedOn = DateTime.Now;
                context.MarkerCategories.Attach(markerCategory);
                context.Refresh(RefreshMode.KeepCurrentValues, markerCategory);
                context.SubmitChanges();
            }

        }

        public void UpdateMarker(Marker marker)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                marker.ModifiedOn = DateTime.Now;
                context.Markers.Attach(marker);
                context.Refresh(RefreshMode.KeepCurrentValues, marker);
                context.SubmitChanges();
            }

        }

        public Marker GetMarker (int Id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.Markers.FirstOrDefault(m => m.Id == Id);
            }
        }

        public MarkerCategory GetMarkerCategory(int Id)
        {
            using (var context = new ManufacturingDataContext(_connectionString))
            {
                return context.MarkerCategories.FirstOrDefault(m => m.Id == Id);
            }
        }
    }
}

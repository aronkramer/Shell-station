using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionTracker.Data
{
    public static class Helpers
    {
        public static EntitySet<T> ToEntitySet<T>(this IEnumerable<T> source) where T : class
        {
            var es = new EntitySet<T>();
            es.AddRange(source);
            return es;
        }
        public static bool NotNull<T> (this T source)
        {
            //if (source.GetType().GetInterfaces().Any(
            //i => i.IsGenericType &&
            //i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            //{
            //    var elementType = source.GetType().GetGenericArguments()[0];
            //    var iernbleSourse = source as List<>;
            //    return iernbleSourse.Count() > 0;
            //}
            return source != null;
        }
        public static bool NotNUllOrEmpty<T>(this IEnumerable<T> source)
        {
            return source != null && source.Count() > 0;
        }
        public static IEnumerable<PlannedProduction> NotDeleted(this IEnumerable<PlannedProduction> source)
        {
            return source.Where(x => !x.Deleted);
        }
    }
}

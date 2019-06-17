using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json;
using System.ComponentModel;

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
        public static bool IsEnumerableType<T>(this T obj)
        {
            
            return obj.GetType().IsEnumerableType();
        }

        public static bool IsEnumerableType(this Type type)
        {
            return type != typeof(string) && type.GetInterface(nameof(IEnumerable)) != null;
        }

        public static bool IsOfGenericType(this Type source)
        {
            List<Type> types = new List<Type>
            {
                typeof(string),
                typeof(int),
                typeof(bool),
                typeof(DateTime),
                typeof(decimal),
                typeof(double),
                typeof(int?),
                typeof(bool?),
                typeof(DateTime?),
                typeof(decimal?),
                typeof(double?)
            };
            return types.Contains(source);
        }
        public static bool IsOfGenericType(this object source)
        {
            return source.GetType().IsOfGenericType();
        }

        public static string GetBasePropertiesOnDbObject<T>(this T obj)
        {
            var dict = new Dictionary<string, object>();
            foreach (var propertyInfo in obj.GetType()
                                .GetProperties(
                                        BindingFlags.Public
                                        | BindingFlags.Instance))
            {
                if (propertyInfo.PropertyType.IsOfGenericType())
                {
                    dict[propertyInfo.Name] = propertyInfo.GetValue(obj, null);
                }
            }
            return JsonConvert.SerializeObject(dict);
        }


        public static T GetObjectBasePropertiesOnDbObject<T>(this T obj)
        {
            return JsonConvert.DeserializeObject<T>(obj.GetBasePropertiesOnDbObject());
        }

        public static bool DefaltOrNull<T>(this T obj)
        {
            //T x = obj;
            //var def = default(T);
            //var result = EqualityComparer<T>.Default.Equals(obj, default(T));
            //return result;
            //return EqualityComparer<T>.Default.Equals(obj, default(T));
            if (obj == null) return true;
            var x = typeof(EqualityComparer<>).MakeGenericType(obj.GetType());
            var t = x.GetProperties(BindingFlags.Static | BindingFlags.Public);
            var defaultEquality = t[0];
            var method = x.GetMethods().First(m => m.Name == "Equals");
            return (bool)method.Invoke(defaultEquality.GetValue(null), new object[] { obj, default(T) });
        }


        public static T SetOrginalDbObjToUpdated<T>(this T original, T updated)
        {
            original = original.GetObjectBasePropertiesOnDbObject();            
            foreach (var propertyInfo in updated.GetType()
                                .GetProperties(
                                        BindingFlags.Public
                                        | BindingFlags.Instance))
            {
                if (propertyInfo.PropertyType.IsOfGenericType())
                {
                    var value = propertyInfo.GetValue(updated,null);
                    if (!value.DefaltOrNull())
                    {
                        propertyInfo.SetValue(original, value, null);
                        
                    }
                }
            }
            return original;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rozmawiator.Database.ViewModels
{
    public class Filter
    {
        public static Filter CreateNew => new Filter();

        public Filter()
        {
            Filters = new Dictionary<string, object>();
        }

        public Filter(Dictionary<string, object> filters)
        {
            Filters = filters;
        }

        public Dictionary<string, object> Filters { get; }

        public object this[string propertyName]
        {
            get { return Filters[propertyName]; }
            set { Filters[propertyName] = value; }
        }

        private static Guid? TryParseToGuid(object value)
        {
            Guid result;
            if (Guid.TryParse(value.ToString(), out result))
            {
                return result;
            }
            return null;
        }

        public IQueryable<TModel> FilterQuery<TModel>(IQueryable<TModel> query)
        {
            var type = query.ElementType;
            var properties = type.GetRuntimeProperties().ToArray();

            foreach (var filter in Filters)
            {
                var property = properties.FirstOrDefault(p => p.Name == filter.Key);
                if (property == null)
                {
                    continue;
                }

                var filterValue = filter.Value;

                filterValue = TryParseToGuid(filterValue) ?? filterValue;

                var param = Expression.Parameter(type);
                Expression<Func<TModel, bool>> condition;

                try
                {
                    condition =
                        Expression.Lambda<Func<TModel, bool>>(
                            Expression.Equal(
                                Expression.Property(param, filter.Key),
                                Expression.Constant(filterValue, filterValue.GetType())
                                ),
                            param
                            );
                }
                catch (InvalidOperationException)
                {
                    return Enumerable.Empty<TModel>().AsQueryable();
                }

                query = query.Where(condition);
            }

            return query;
        }
    }
}

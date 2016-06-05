using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Database.ViewModels
{
    public static class FilterExtensions
    {
        public static Filter Set(this Filter filter, string key, object value)
        {
            filter["key"] = value;
            return filter;
        }
    }
}

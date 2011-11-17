using System;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.Bootstrap.Clarify.Extensions
{
    public static class ClarifyGenericExtensions
    {
        public static ClarifyGeneric Filter(this ClarifyGeneric generic, Func<FilterExpression, Filter> filterFunction)
        {
            var filterExpression = new FilterExpression();

            var filter = filterFunction(filterExpression);

            generic.Filter.AddFilter(filter);

            return generic;
        }
    }
}
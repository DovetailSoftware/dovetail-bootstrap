using System;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.Clarify
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

        public static ClarifyGeneric TraverseWithFields(this ClarifyGeneric generic, string relationName, params string[] fields)
        {
            if (fields == null || fields.Length < 1)
                fields = new[] {"objid"};

            var childGeneric = generic.Traverse(relationName);

            childGeneric.DataFields.AddRange(fields);

            return childGeneric;
        }

        public static ClarifyGeneric CreateGenericWithFields(this ClarifyDataSet dataSet, string objectName,  params string[] fields)
        {
            if (fields == null || fields.Length < 1)
                fields = new[] { "objid" };

            var generic = dataSet.CreateGeneric(objectName);
            generic.DataFields.AddRange(fields);

            return generic;
        }
    }
}
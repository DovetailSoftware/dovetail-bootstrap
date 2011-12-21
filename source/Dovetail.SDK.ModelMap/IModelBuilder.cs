using System;
using System.Collections.Generic;
using Dovetail.SDK.ModelMap.ObjectModel;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.ModelMap
{
    public interface IModelBuilder<MODEL>
    {
        MODEL[] Get(Filter filter);
        MODEL[] Get(Func<FilterExpression, Filter> filterFunction);
        MODEL GetOne(string identifier);
        MODEL GetOne(int identifier);
        MODEL[] GetTop(Filter filter, int dtoCountLimit);
        MODEL[] GetTop(Func<FilterExpression, Filter> filterFunction, int dtoCountLimit);
    	MODEL[] GetAll();
    	IEnumerable<FieldSortMap> FieldSortMapOverrides { get; set; }
    	MODEL[] GetAll(int dtoCountLimit);
    }
}
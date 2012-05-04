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
		MODEL[] GetAll();
		PaginatedResults<MODEL> Get(Filter filter, IPagination pagination);
		PaginatedResults<MODEL> Get(Func<FilterExpression, Filter> filterFunction, IPagination pagination);
    	IEnumerable<FieldSortMap> FieldSortMapOverrides { get; set; }
    	MODEL[] GetAll(int dtoCountLimit);
    }
}
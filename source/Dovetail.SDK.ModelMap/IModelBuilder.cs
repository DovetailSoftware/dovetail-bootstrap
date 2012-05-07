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
		PaginationResult<MODEL> Get(Filter filter, IPaginationRequest paginationRequest);
		PaginationResult<MODEL> Get(Func<FilterExpression, Filter> filterFunction, IPaginationRequest paginationRequest);
    	IEnumerable<FieldSortMap> FieldSortMapOverrides { get; set; }
    	MODEL[] GetAll(int dtoCountLimit);
    }
}
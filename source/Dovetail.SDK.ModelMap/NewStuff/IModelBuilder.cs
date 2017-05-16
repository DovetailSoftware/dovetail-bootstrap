using System;
using System.Collections.Generic;
using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public interface IModelBuilder
    {
		IEnumerable<FieldSortMap> FieldSortMapOverrides { get; set; }

		ModelData GetOne(string name, int identifier);
		ModelData GetOne(string name, string identifier);

		ModelData[] Get(string name, Filter filter);
		ModelData[] Get(string name, Func<FilterExpression, Filter> filterFunction);
		ModelData[] GetTop(string name, Filter filter, int numberOfRecords);
		ModelData[] GetTop(string name, Func<FilterExpression, Filter> filterFunction, int numberOfRecords);

		PaginationResult Get(string name, Filter filter, IPaginationRequest paginationRequest);
		PaginationResult Get(string name, Func<FilterExpression, Filter> filterFunction, IPaginationRequest paginationRequest);
		
		ModelData[] GetAll(string name);
		ModelData[] GetAll(string name, int dtoCountLimit);
	}
}
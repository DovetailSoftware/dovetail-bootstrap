using System;
using System.Collections.Generic;
using Dovetail.SDK.ModelMap.ObjectModel;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.ModelMap
{
    public interface IModelBuilder<TModel>
    {
		/// <summary>
		/// Get models based on the ModelMap of TModel present in the container.
		/// </summary>
		/// <param name="filter">Query filter for the map's root generic table or view.</param>
		/// <returns>Array of models matching the given filter</returns>
        TModel[] Get(Filter filter);

		/// <summary>
		/// Get models based on the ModelMap of TModel present in the container.
		/// </summary>
		/// <param name="filterFunction">Query filter for the map's root generic table or view.</param>
		/// <returns>Array of models matching the given filter</returns>
		TModel[] Get(Func<FilterExpression, Filter> filterFunction);

		/// <summary>
		/// Get the top N records. This is syntatical suger over the Get with a pagination request version of this API.
		/// </summary>
		/// <param name="filter">Query filter for the map's root generic table or view.</param>
		/// <param name="numberOfRecords">How many records off the top of the result set should be returned.</param>
		/// <returns>Array of model objects</returns>
		TModel[] GetTop(Filter filter, int numberOfRecords);

		/// <summary>
		/// Get the top N records. This is syntatical suger over the Get with a pagination request version of this API.
		/// </summary>
		/// <param name="filterFunction">Query filter for the map's root generic table or view.</param>
		/// <param name="numberOfRecords">How many records off the top of the result set should be returned.</param>
		/// <returns></returns>
		TModel[] GetTop(Func<FilterExpression, Filter> filterFunction, int numberOfRecords);

		/// <summary>
		/// Get one model based on the identifying field of the model map
		/// </summary>
		/// <param name="identifier">String identifier</param>
		/// <returns>Model identified by the given identifier when presend.</returns>
		TModel GetOne(string identifier);

		/// <summary>
		/// Get one model based on the identifying field of the model map
		/// </summary>
		/// <param name="identifier">Integer identifier</param>
		/// <returns>Model identified by the given identifier when presend.</returns>
		TModel GetOne(int identifier);

		/// <summary>
		/// Get models by filter with pagination.
		/// </summary>
		/// <param name="filter">Query filter for the map's root generic table or view.</param>
		/// <param name="paginationRequest">Details about how big the starting page and how big the page returned should be</param>
		/// <returns>Model array from the requested subset of the result set</returns>
		PaginationResult<TModel> Get(Filter filter, IPaginationRequest paginationRequest);

		/// <summary>
		/// Get models by filter with pagination.
		/// </summary>
		/// <param name="filterFunction">Query filter for the map's root generic table or view.</param>
		/// <param name="paginationRequest">Details about how big the starting page and how big the page returned should be</param>
		/// <returns>Model array from the requested subset of the result set</returns>
		PaginationResult<TModel> Get(Func<FilterExpression, Filter> filterFunction, IPaginationRequest paginationRequest);
    	IEnumerable<FieldSortMap> FieldSortMapOverrides { get; set; }

		/// <summary>
		/// Get all resulting models returned for the TModel's map. If the ModelMap itself does not have a filter applied all records the the map's source table or view will be returned.
		/// </summary>
		/// <returns>Array of all models resulting from the model map.</returns>
		TModel[] GetAll();

		/// <summary>
		/// Get all resulting models returned for the TModel's map. If the ModelMap itself does not have a filter applied all records the the map's source table or view will be returned.
		/// </summary>
		/// <param name="dtoCountLimit">Limit the number of models returned</param>
		/// <returns>Limited array of all models resulting from the model map</returns>
		TModel[] GetAll(int dtoCountLimit);
    }
}
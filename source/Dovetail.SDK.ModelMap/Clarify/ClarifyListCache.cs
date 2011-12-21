using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.ModelMap.ObjectModel;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Clarify.DataObjects;

namespace Dovetail.SDK.ModelMap.Clarify
{
	public class ClarifyListCache : IClarifyListCache
    {
        private readonly IListCache _listCache;
        private readonly ILocaleCache _localeCache;

        public ClarifyListCache(IListCache listCache, ILocaleCache localeCache)
        {
            _listCache = listCache;
            _localeCache = localeCache;
        }

        public IClarifyList GetList(string listName)
        {
            var gbstList = _listCache.GetGbstList(listName);

            if (gbstList == null)
            {
                throw new ApplicationException(String.Format("Gbst list {0} was not found in the list cache.", listName));
            }

        	var listElements = from GlobalStringElement element in gbstList.ActiveElements
        	                             let isElementDefault = element.Equals(gbstList.DefaultElement)
        	                             select new ClarifyListElement(element.Title, element.Rank, isElementDefault) { DatabaseIdentifier = element.ObjectID };

        	return new ClarifyGlobalList(gbstList.Title, listElements.ToArray());
        }

		public string GetListElement(string listName, int elementDatabaseIdentifier)
		{
			return _listCache.GetGbstElmByID(listName, elementDatabaseIdentifier);
		}

		public IClarifyList GetHgbstList(string listName)
        {
            return GetHgbstList(new UserDefinedList(listName));
        }

        public IClarifyList GetHgbstList(UserDefinedList userDefinedList)
        {
            var hgbstList = _listCache.GetHgbstList(userDefinedList.ListName, userDefinedList.ListValues);

            if (hgbstList == null)
            {
                throw new ApplicationException(String.Format("Hgbst list {0} was not found in the list cache.", userDefinedList.ListName));
            }

            var clarifyListElements = hgbstList.Cast<HierarchicalStringElement>()
                                            .Where(element => element.IsActive)
                                            .Select(element => new ClarifyListElement(element.Title, element.Rank, element.IsDefault) { DatabaseIdentifier = element.ObjectID });

            return new ClarifyGlobalList(userDefinedList.ListName, clarifyListElements.ToArray());
        }

        public IEnumerable<Location> GetStates(string countryName)
        {
            if (!_localeCache.IsCountry(countryName))
            {
                throw new ApplicationException(String.Format("Country {0} is not present in the list cache.", countryName));
            }

            return _localeCache.GetStates(countryName)
                        .Cast<StateProvince>()
                        .Select(state => new Location { IsDefault = state.IsDefault, FullName = state.FullName, Name = state.Name });
        }

        public IEnumerable<Location> GetCountries()
        {
        	return _localeCache.Countries
        		.Cast<Country>()
        		.OrderByDescending(x => x.IsDefault)
				.ThenBy(x=>x.Name)
        		.Select(country => new Location {IsDefault = country.IsDefault, Name = country.Name});
        }
    }
}

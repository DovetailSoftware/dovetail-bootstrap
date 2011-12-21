using System.Collections.Generic;
using Dovetail.SDK.ModelMap.ObjectModel;

namespace Dovetail.SDK.ModelMap.Clarify
{
    public interface IClarifyListCache
    {
        IClarifyList GetList(string listName);
        IClarifyList GetHgbstList(string listName);
        IClarifyList GetHgbstList(UserDefinedList userDefinedList);
        IEnumerable<Location> GetStates(string countryName);
        IEnumerable<Location> GetCountries();
    	string GetListElement(string listName, int elementDatabaseIdentifier);
    }
}

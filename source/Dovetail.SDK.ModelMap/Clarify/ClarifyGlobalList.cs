using System.Collections;
using System.Collections.Generic;

namespace Dovetail.SDK.ModelMap.Clarify
{
    public class ClarifyGlobalList : IClarifyList
    {
    	private readonly IEnumerable<IClarifyListElement> _elements;
        public ClarifyGlobalList(string title, IEnumerable<IClarifyListElement> elements)
        {
            _elements = elements;
            Title = title;
        }

		public string Title { get; private set; }

    	public IEnumerator<IClarifyListElement> GetEnumerator()
    	{
    		return _elements.GetEnumerator();
    	}

    	IEnumerator IEnumerable.GetEnumerator()
    	{
    		return GetEnumerator();
    	}
    }
}
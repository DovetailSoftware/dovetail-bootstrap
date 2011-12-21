using System.Collections.Generic;

namespace Dovetail.SDK.ModelMap.Clarify
{
    public interface IClarifyList : IEnumerable<IClarifyListElement>
    {
        string Title { get; }
    }
}
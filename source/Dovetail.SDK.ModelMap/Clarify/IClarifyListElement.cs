namespace Dovetail.SDK.ModelMap.Clarify
{
    public interface IClarifyListElement
    {
        int DatabaseIdentifier { get; }
        string Title { get; }
		string DisplayTitle { get; set; }
        int Rank { get; }
        bool IsDefault { get; }
        bool IsSelected { get; set; }
    }
}
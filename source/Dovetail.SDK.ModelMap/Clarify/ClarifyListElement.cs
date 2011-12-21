namespace Dovetail.SDK.ModelMap.Clarify
{
    public class ClarifyListElement : IClarifyListElement
    {
        public string Title { get; private set; }
        public int Rank { get; private set; }
        public bool IsDefault { get; private set; }
        public bool IsSelected { get; set; }
        public int DatabaseIdentifier { get; set; }
		public string DisplayTitle { get; set; }

        public ClarifyListElement(string title, int rank, bool isDefault)
        {
            Title = title;
        	DisplayTitle = title;
            Rank = rank;
            IsDefault = isDefault;
        }
    }
}
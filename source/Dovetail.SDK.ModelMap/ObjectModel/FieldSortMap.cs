namespace Dovetail.SDK.ModelMap.ObjectModel
{
    public class FieldSortMap
    {
        public string FieldName { get; set; }
        public bool IsAscending { get; set; }

        public FieldSortMap(string fieldName, bool isAscending)
        {
            FieldName = fieldName;
            IsAscending = isAscending;
        }
    }
}
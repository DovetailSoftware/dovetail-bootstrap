namespace Dovetail.SDK.ModelMap.Clarify
{
    public class UserDefinedList
    {
        public string ListName { get; private set; }
        public string[] ListValues { get; private set; }

        public UserDefinedList(string listName)
        {
            ListName = listName;
            ListValues = new string[0];
        }

        public UserDefinedList(string listName, string[] listValues)
        {
            ListName = listName;
            ListValues = listValues;
        }
    }
}
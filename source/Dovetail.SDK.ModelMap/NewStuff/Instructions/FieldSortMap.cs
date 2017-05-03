namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class FieldSortMap : IModelMapInstruction
    {
        public string FieldName { get; set; }
        public bool IsAscending { get; set; }

        public FieldSortMap(string fieldName, bool isAscending)
        {
            FieldName = fieldName;
            IsAscending = isAscending;
        }

        public void Accept(IModelMapVisitor visitor)
        {
            
        }
    }
}
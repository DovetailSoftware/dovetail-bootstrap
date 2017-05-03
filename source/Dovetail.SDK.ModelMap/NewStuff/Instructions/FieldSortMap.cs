using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class FieldSortMap : IModelMapInstruction
    {
        public string Field { get; set; }
        public string Type { get; set; }
        public bool IsAscending { get { return "asc".EqualsIgnoreCase(Type); } }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
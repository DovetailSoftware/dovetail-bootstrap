namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class BeginProperty : IModelMapInstruction
    {
        public string Key { get; set; }
        public string Field { get; set; }
        public string DataType { get; set; }
        public string PropertyType { get; set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
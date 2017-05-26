namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class BeginProperty : IModelMapInstruction
    {
        public IDynamicValue Key { get; set; }
        public IDynamicValue Field { get; set; }
        public IDynamicValue DataType { get; set; }
        public string PropertyType { get; set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
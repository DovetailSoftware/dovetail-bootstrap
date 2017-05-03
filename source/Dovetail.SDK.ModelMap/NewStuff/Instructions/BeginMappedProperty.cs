namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class BeginMappedProperty : IModelMapInstruction
    {
        public string Key { get; set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
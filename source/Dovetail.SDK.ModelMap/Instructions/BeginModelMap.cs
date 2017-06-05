namespace Dovetail.SDK.ModelMap.Instructions
{
    public class BeginModelMap : IModelMapInstruction
    {
        public BeginModelMap(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
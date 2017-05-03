namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class BeginView : IModelMapInstruction, IQueryContext
    {
        public BeginView(string viewName)
        {
            ViewName = viewName;
        }

        public string ViewName { get; private set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
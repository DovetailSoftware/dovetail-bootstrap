namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class BeginTable : IModelMapInstruction, IQueryContext
    {
        public BeginTable(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; private set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
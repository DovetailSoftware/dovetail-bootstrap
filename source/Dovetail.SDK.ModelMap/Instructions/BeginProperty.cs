namespace Dovetail.SDK.ModelMap.Instructions
{
    public class BeginProperty : IModelMapInstruction
    {
        public IDynamicValue Key { get; set; }
        public IDynamicValue Field { get; set; }
        public IDynamicValue DataType { get; set; }
        public string PropertyType { get; set; }

	    public bool IsIdentifier
	    {
		    get { return PropertyType == "identifier"; }
	    }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }

	    public override string ToString()
	    {
		    return "Begin " + Key;
	    }
    }
}
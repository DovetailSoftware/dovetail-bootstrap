using System;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.ModelMap.NewStuff.Instructions
{
    public class AddFilter : IModelMapInstruction, ICloneable
    {
        public AddFilter(Filter filter)
        {
            Filter = filter;
        }

        public Filter Filter { get; private set; }

        public void Accept(IModelMapVisitor visitor)
        {
            visitor.Visit(this);
        }

	    public object Clone()
	    {
		    return new AddFilter(Filter);
	    }
    }
}
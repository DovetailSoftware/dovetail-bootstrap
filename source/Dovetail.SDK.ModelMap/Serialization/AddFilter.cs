using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FubuCore;

namespace Dovetail.SDK.ModelMap.Serialization
{
    public class AddFilter : IElementVisitor
    {
        public bool Matches(XElement element, ModelMap map, ParsingContext context)
        {
            var registry = context.Service<IFilterPolicyRegistry>();
            return context.IsCurrent<AddFilterContext>() && registry.HasPolicy(element.Name.ToString());
        }

        public void Visit(XElement element, ModelMap map, ParsingContext context)
        {
            var registry = context.Service<IFilterPolicyRegistry>();
            var policyType = registry.FindPolicy(element.Name.ToString());
            var builder = context.Service<IObjectBuilder>();
            var values = new Dictionary<string, string>(element
                    .Attributes()
                    .ToDictionary(_ => _.Name.ToString(), _ => context.Serializer.ValueFor(_).ToString()));

            var result = builder.Build(new BuildObjectContext(policyType, values));

            if (result.HasErrors())
            {
                var missingAttributes = result.Errors.Select(_ => _.Key);
                throw new ModelMapException("The {0} attribute(s) must be specified on the {1} element".ToFormat(missingAttributes.Join(","), element.Name));
            }

            var policy = result.Result.As<IFilterPolicy>();
            map.AddInstruction(new Instructions.AddFilter(policy.CreateFilter()));
        }

        public void ChildrenBound(ModelMap map, ParsingContext context)
        {
        }
    }

    public class AddFilterContext
    {
    }

    public class Where : IElementVisitor
    {
        public bool Matches(XElement element, ModelMap map, ParsingContext context)
        {
            return element.Name == "where";
        }

        public void Visit(XElement element, ModelMap map, ParsingContext context)
        {
            context.PushObject(new AddFilterContext());
        }

        public void ChildrenBound(ModelMap map, ParsingContext context)
        {
            context.PopObject();
        }
    }
}
using System.Collections.Generic;
using System.Linq;

namespace Dovetail.SDK.ModelMap.Serialization
{
    public class ObjectBuilder : IObjectBuilder
    {
        public ObjectBuilderResult Build(BuildObjectContext context)
        {
            var constructor = context
                .Type
                .GetConstructors()
                .OrderByDescending(_ => _.GetParameters().Length)
                .First();

            var result = new ObjectBuilderResult();
            var parameters = constructor.GetParameters();
            var arguments = new List<object>();
            var usedValues = new List<string>();

            if (parameters.Any())
            {
                parameters
                    .Where(_ => !context.Has(_.Name))
                    .Each(_ => result.AddError(_.Name, "Missing required parameter"));

                if (result.HasErrors())
                    return result;

                parameters
                    .Select(_ => new
                    {
                        Name = _.Name,
                        Value = context.GetValue(_.Name, _.ParameterType)
                    })
                    .Each(_ =>
                    {
                        usedValues.Add(_.Name.ToLower());
                        arguments.Add(_.Value);
                    });
            }

            var target = constructor.Invoke(arguments.ToArray());

            // Include properties
            var properties = context
                .Type
                .GetProperties()
                .Where(_ => _.CanWrite && context.Has(_.Name) && !usedValues.Contains(_.Name.ToLower()))
                .ToArray();

            foreach (var prop in properties)
                prop.SetValue(target, context.GetValue(prop.Name, prop.PropertyType));

            result.Result = target;
            return result;
        }
    }
}
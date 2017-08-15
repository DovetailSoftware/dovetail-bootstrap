using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FubuCore;
using FubuCore.Reflection;

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
                    .Where(_ => !context.Has(_.Name) && !(_.ParameterType.IsArray && _.HasAttribute<ParamArrayAttribute>()))
                    .Each(_ => result.AddError(_.Name, "Missing required parameter"));

                if (result.HasErrors())
                    return result;

                parameters
					.Where(_ => !(_.ParameterType.IsArray && _.HasAttribute<ParamArrayAttribute>()))
					.Select(_ => new
                    {
                        _.Name,
                        Value = context.GetValue(_.Name, _.ParameterType)
                    })
                    .Each(_ =>
                    {
                        usedValues.Add(_.Name.ToLower());
                        arguments.Add(_.Value);
                    });

	            if (parameters.Any(_ => _.ParameterType.IsArray && _.HasAttribute<ParamArrayAttribute>()))
	            {
		            var param = parameters.First(_ => _.ParameterType.IsArray && _.HasAttribute<ParamArrayAttribute>());
		            var type = param.ParameterType.FindInterfaceThatCloses(typeof(IEnumerable<>)).GetGenericArguments()[0];
		            var builder = typeof(ParamsBuilder<>).CloseAndBuildAs<IParamsBuilder>(type);
					arguments.Add(builder.BuildParams(context));
	            }
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

	    private interface IParamsBuilder
	    {
		    object BuildParams(BuildObjectContext context);
	    }

	    private class ParamsBuilder<T> : IParamsBuilder
	    {
		    public object BuildParams(BuildObjectContext context)
		    {
			    return context.GetParamValues<T>().ToArray();
		    }
	    }
    }
}
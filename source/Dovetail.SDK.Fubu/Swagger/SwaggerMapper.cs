using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using FubuCore.Reflection;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.Routes;

namespace Dovetail.SDK.Fubu.Swagger
{
    public interface ISwaggerMapper
    {
        IEnumerable<Operation> OperationsFrom(ActionCall call);
    }

    public class SwaggerMapper : ISwaggerMapper
    {
        private readonly ITypeDescriptorCache _typeCache;

        public SwaggerMapper(ITypeDescriptorCache typeCache)
        {
            _typeCache = typeCache;
        }

        public IEnumerable<Operation> OperationsFrom(ActionCall call)
        {
            //HACK assuming that there is one verb allowed per action - big assumption :(
            var route = call.ParentChain().Route;
            var httpMethods = route.AllowedHttpMethods;
            
            var parameters = getParameters(call);
            var outputType = call.OutputType();

            
            var operations = new List<Operation>();
            foreach (var verb in httpMethods)
            {
                var operation = new Operation
                                    {
                                        parameters = parameters.ToArray(),
                                        httpMethod = verb,
                                        responseTypeInternal = outputType.FullName,
                                        responseClass = outputType.Name,
                                        nickname = call.InputType().Name,
                                        notes = "notes",
                                        errorResponses = new ErrorResponses[0],
                                        //TODO get notes, nickname, summary from metadata?
                                    };
                operations.Add(operation);
            }
            return operations;
        }

        private IEnumerable<Parameter> getParameters(ActionCall call)
        {
            if (!call.HasInput) return new Parameter[0];

            var inputType = call.InputType();
            IEnumerable<PropertyInfo> properties = _typeCache.GetPropertiesFor(inputType).Values;
            var route = call.ParentChain().Route;

            return properties.Select(propertyInfo => createParameterFromProperty(propertyInfo, route));
        }

        private static Parameter createParameterFromProperty(PropertyInfo propertyInfo, IRouteDefinition route)
        {
            var parameter = new Parameter
                                {
                                    name = propertyInfo.Name,
                                    dataType = propertyInfo.PropertyType.Name,
                                    paramType = "post",
                                    allowMultiple = false,
                                    required = propertyInfo.HasAttribute<RequiredAttribute>(),
                                    description = propertyInfo.GetAttribute<DescriptionAttribute>(a => a.Description),
                                    defaultValue = propertyInfo.GetAttribute<DefaultValueAttribute>(a => a.Value.ToString()),
                                    allowableValues = getAllowableValues(propertyInfo)
                                };

            if (route.Input.RouteParameters.Any(r => r.Name == propertyInfo.Name))
                parameter.paramType = "path";

            if (route.Input.QueryParameters.Any(r => r.Name == propertyInfo.Name))
                parameter.paramType = "query";

            return parameter;
        }

        private static AllowableValues getAllowableValues(ICustomAttributeProvider propertyInfo)
        {
            var allowableValues = propertyInfo.GetAttribute<AllowableValuesAttribute>();

            if(allowableValues == null)
                return null;

            return new AllowableValues {valueType = "LIST", values = allowableValues.AllowableValues};
        }
    }
}
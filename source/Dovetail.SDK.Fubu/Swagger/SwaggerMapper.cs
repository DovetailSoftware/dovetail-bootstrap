using System.Collections.Generic;
using System.Linq;
using FubuCore.Reflection;
using FubuMVC.Core.Registration.Nodes;

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
                                        responseTypeInternal = outputType.AssemblyQualifiedName,
                                        responseClass = outputType.Name,
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
            var properties = _typeCache.GetPropertiesFor(inputType).Values;
            var route = call.ParentChain().Route;

            var parameters = new List<Parameter>();
            foreach (var propertyInfo in properties)
            {
                var parameter = new Parameter
                                    {
                                        name = propertyInfo.Name,
                                        dataType = propertyInfo.PropertyType.Name,
                                        parameterType = "post",
                                        allowMultiple = false,
                                        //TODO get defaultValue, description, allowableValues from metadata?
                                        
                                    };

                if(route.Input.RouteParameters.Any(r=>r.Name == propertyInfo.Name))
                    parameter.parameterType = "path";

                if (route.Input.QueryParameters.Any(r => r.Name == propertyInfo.Name))
                    parameter.parameterType = "query";

                parameters.Add(parameter);
            }
            return parameters;
        }
    }
}
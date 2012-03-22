using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Fubu.Actions;
using Dovetail.SDK.Fubu.TokenAuthentication.Token.Extensions;
using FubuMVC.Core.Registration;

namespace Dovetail.SDK.Fubu.TokenAuthentication.Token
{
    public class APIExceptionConvention<TServerErrorRequest> : IConfigurationAction
        where TServerErrorRequest : class, IServerErrorRequest, new()
    {
        public void Configure(BehaviorGraph graph)
        {
            graph
                .Actions()
                .Where(action => action.InputType().IsAPIRequest())
                .Each(action => action.WrapWith<ActionExceptionWrapper<TServerErrorRequest>>());
        }
    }
}
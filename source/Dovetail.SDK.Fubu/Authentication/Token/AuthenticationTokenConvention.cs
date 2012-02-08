using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap;
using FubuCore;
using FubuMVC.Core.Registration;

namespace Dovetail.SDK.Fubu.Authentication.Token
{
    public class AuthenticationTokenConvention : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            graph
                .Actions()
                .Where(action => action.InputType().CanBeCastTo<IApi>())
                .Each(action => action
                    .ParentChain()
                    .Authorization
                    .AddPolicy(typeof(AuthenticationTokenAuthorizationPolicy)));
        }
    }

}
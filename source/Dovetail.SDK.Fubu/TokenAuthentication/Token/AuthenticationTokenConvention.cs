using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Fubu.TokenAuthentication.Token.Extensions;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Resources.Conneg;

namespace Dovetail.SDK.Fubu.TokenAuthentication.Token
{
    public class AuthenticationTokenConvention : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            graph.Behaviors
                .Where(action => action.InputType().IsAPIRequest())
                .Each(action => action.MakeAsymmetricJson());

            graph
                .Actions()
                .Where(action => action.InputType().IsAuthenticatedAPIRequest())
                .Each(action => 
                        action.ParentChain().Authorization.AddPolicy(typeof(AuthenticationTokenAuthorizationPolicy))
                    );
        }
    }
}
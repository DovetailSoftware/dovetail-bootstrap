using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap;
using FubuCore;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Resources.Conneg;

namespace Dovetail.SDK.Fubu.Authentication.Token
{
    public class AuthenticationTokenConvention : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            graph.Behaviors
                .Where(x => x.InputType().CanBeCastTo<IApi>() || x.InputType().CanBeCastTo<IUnauthenticatedApi>())
                .Each(x => x.MakeAsymmetricJson());

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
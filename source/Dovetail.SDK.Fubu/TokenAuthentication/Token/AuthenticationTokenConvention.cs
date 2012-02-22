using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap;
using FubuCore;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Resources.Conneg;

namespace Dovetail.SDK.Fubu.TokenAuthentication.Token
{
    public interface IApi { }

    public class AuthenticationTokenConvention : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            graph.Behaviors
                .Where(x => TypeExtensions.CanBeCastTo<IApi>(x.InputType()) || TypeExtensions.CanBeCastTo<IUnauthenticatedApi>(x.InputType()))
                .Each(x => BehaviorChainConnegExtensions.MakeAsymmetricJson(x));

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
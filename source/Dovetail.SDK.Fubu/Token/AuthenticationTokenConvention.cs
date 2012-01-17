using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap;
using FubuCore;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;

namespace Dovetail.SDK.Fubu.Token
{
    public class AuthenticationTokenConvention : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            graph
                .Actions()
                .Where(action => action.InputType().CanBeCastTo<IApi>())
                .Each(call =>
                {
                    var log = graph.Observer;
                    if (log.IsRecording)
                    {
                        log.RecordCallStatus(call, "{0} has an output model that requires an authentication token be present. Wrapping with AuthenticationTokenBehavior.".ToFormat(call));
                    }

                    call.AddBefore(new Wrapper(typeof(AuthenticationTokenBehavior)));
                });
        }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dovetail.SDK.Bootstrap;
using FubuCore;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;

namespace Dovetail.SDK.Fubu.Swagger
{
    public interface IApiFinder
    {
        IEnumerable<ActionCall> Actions();
        IEnumerable<ActionCall> ActionsForGroup(string name);
        IEnumerable<IGrouping<string, ActionCall>> ActionsByGroup();
    }

    public class ApiFinder : IApiFinder
    {
        private readonly BehaviorGraph _graph;
        private readonly Type _apiMakerType = typeof (IApi);

        public ApiFinder(BehaviorGraph graph)
        {
            _graph = graph;
        }

        public IEnumerable<ActionCall> Actions()
        {
            return _graph.Actions().Where(a => a.InputType().CanBeCastTo(_apiMakerType)); 
        }

        public IEnumerable<IGrouping<string, ActionCall>> ActionsByGroup()
        {
            var partGroupExpression = new Regex(@"^/?api/(?<group>[a-zA-Z0-9_\-\.\+]+)/?");

            //TODO might want to cache the intermediate result

            var groups = Actions()
                .GroupBy(a =>
                             {
                                 var pattern = a.ParentChain().Route.Pattern;
                                 var match = partGroupExpression.Match(pattern);
                                 return match.Success ? match.Groups["group"].Value : null;
                             });
            
            return groups.ToArray();
        }

        public IEnumerable<ActionCall> ActionsForGroup(string name)
        {
            var group = ActionsByGroup().FirstOrDefault(g => g.Key.ToLowerInvariant() == name);

            return @group ?? (IEnumerable<ActionCall>) new ActionCall[0];
        }
    }
}
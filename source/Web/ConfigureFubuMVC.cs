using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Bootstrap.Web.Handlers;
using Bootstrap.Web.Handlers.error.http500;
using Bootstrap.Web.Security;
using Dovetail.SDK.Fubu.Clarify.Lists;
using Dovetail.SDK.Fubu.TokenAuthentication.Token;
using FubuCore;
using FubuCore.Descriptions;
using FubuCore.Reflection;
using FubuLocalization;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Conventions;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.Routes;
using FubuMVC.Core.Security;
using FubuMVC.Localization;
//using FubuMVC.Swagger;
//using FubuMVC.Swagger.Configuration;
//using IApi = Dovetail.SDK.Bootstrap.IApi;

namespace Bootstrap.Web
{
	public class ConfigureFubuMVC : FubuRegistry
	{
		public ConfigureFubuMVC()
		{
			Import<HandlerConvention>(x => x.MarkerType<HandlerMarker>());
			Actions.IncludeClassesSuffixedWithEndpoint();

			Policies.Global.Add<AuthenticationTokenConvention>();

			//convention to transfer exceptions to the view for an input model given via generic argument
			Policies.Global.Add<APIExceptionConvention<Error500Request>>();

			Import<BootstrapHtmlConvention>();

			//TODO replace this with Swagger Bottle
			//Policies.Global.Add<SwaggerConvention>();

			Services(s =>
			{
				s.ReplaceService<IAuthorizationFailureHandler, BootstrapAuthorizationFailureHandler>();

//				s.AddService<IActionGrouper, APIRouteGrouper>();
//				s.AddService<IActionFinder, BootstrapActionFinder>();
				s.AddService<ICurrentCultureContext, CurrentCultureContext>();
			});

			Import<BasicLocalizationSupport>();
			Policies.Global.Add(x => x.Wrap.WithBehavior<BasicLocalizationBehavior>());
		}
	}

	//public class BootstrapActionFinder : IActionFinder
	//{
	//	public Func<ActionCall, bool> Matches
	//	{
	//		get { return IsApiCall; }
	//	}

	//	private static bool IsApiCall(ActionCall actionCall)
	//	{
	//		return actionCall.ParentChain().InputType().CanBeCastTo<IApi>();
	//	}
	//}

	public class HandlerConvention : IFubuRegistryExtension
	{
		private readonly IList<Type> _markerTypes = new List<Type>();

		public void MarkerType<T>()
		{
			_markerTypes.Fill(typeof(T));
		}

		public Func<Type[], HandlersUrlPolicy> PolicyBuilder = types => new HandlersUrlPolicy(types);

		void IFubuRegistryExtension.Configure(FubuRegistry registry)
		{
			IEnumerable<Type> markers = _markerTypes.Any()
											? _markerTypes
											: new Type[] { registry.GetType() };

			var source = new HandlerActionSource(markers);
			registry.AlterSettings<UrlPolicies>(urls =>
			{
				urls.Policies.Insert(0, PolicyBuilder(markers.ToArray()));
			});
			registry.Actions.FindWith(source);
		}


		public class HandlerActionSource : ActionSource
		{
			private readonly IEnumerable<Type> _markerTypes;

			public HandlerActionSource(IEnumerable<Type> markerTypes)
			{
				markerTypes.Each(markerType =>
				{
					Applies.ToAssembly(markerType.Assembly);
					IncludeTypes(x => x.Namespace.IsNotEmpty() && x.Namespace.StartsWith(markerType.Namespace));
					IncludeMethods(x => x.Name == HandlersUrlPolicy.METHOD);
				});

				_markerTypes = markerTypes;
			}

			public bool Equals(HandlerActionSource other)
			{
				if (ReferenceEquals(null, other)) return false;
				if (ReferenceEquals(this, other)) return true;

				if (other._markerTypes.Count() != _markerTypes.Count()) return false;

				for (var i = 0; i < _markerTypes.Count(); i++)
				{
					if (other._markerTypes.ElementAt(i) != _markerTypes.ElementAt(i)) return false;
				}

				return true;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != typeof(HandlerActionSource)) return false;
				return Equals((HandlerActionSource)obj);
			}

			public override int GetHashCode()
			{
				return (_markerTypes != null ? _markerTypes.GetHashCode() : 0);
			}
		}

		public void MarkerType(Type markerType)
		{
			_markerTypes.Fill(markerType);
		}
	}

	[Title("HandlerUrlPolicy")]
	public class HandlersUrlPolicy : IUrlPolicy
	{
		public const string HANDLER = "Handler";
		public const string METHOD = "Execute";
		public static readonly Regex HandlerExpression = new Regex("_[hH]andler", RegexOptions.Compiled);

		private readonly IEnumerable<Type> _markerTypes;

		public HandlersUrlPolicy(params Type[] markerTypes)
		{
			_markerTypes = markerTypes;
		}

		public virtual bool Matches(ActionCall call)
		{
			return IsHandlerCall(call);
		}

		public virtual IRouteDefinition Build(ActionCall call)
		{
			var routeDefinition = call.ToRouteDefinition();
			var strippedNamespace = stripNamespace(call);

			visit(routeDefinition);

			if (strippedNamespace != call.HandlerType.Namespace)
			{
				if (!strippedNamespace.Contains("."))
				{
					routeDefinition.Append(breakUpCamelCaseWithHypen(strippedNamespace));
				}
				else
				{
					var patternParts = strippedNamespace.Split(new[] { "." }, StringSplitOptions.None);
					foreach (var patternPart in patternParts)
					{
						routeDefinition.Append(breakUpCamelCaseWithHypen(patternPart.Trim()));
					}
				}
			}

			var handlerName = call.HandlerType.Name;
			var match = HandlerExpression.Match(handlerName);
			if (match.Success && MethodToUrlBuilder.Matches(handlerName))
			{
				// We're forcing handlers to end with "_handler" in this case
				handlerName = handlerName.Substring(0, match.Index);
				var properties = call.HasInput
									 ? new TypeDescriptorCache().GetPropertiesFor(call.InputType()).Keys
									 : new string[0];


				MethodToUrlBuilder.Alter(routeDefinition, handlerName, properties);
			}
			else
			{
				// Otherwise we're expecting something like "GetHandler"
				var httpMethod = call.HandlerType.Name.Replace(HANDLER, string.Empty);
				routeDefinition.ConstrainToHttpMethods(httpMethod.ToUpper());
			}

			if (call.HasInput)
			{
				routeDefinition.ApplyInputType(call.InputType());
			}

			return routeDefinition;
		}

		protected virtual void visit(IRouteDefinition routeDefinition)
		{
			// no-op
		}

		private string stripNamespace(ActionCall call)
		{
			var strippedNamespace = "";

			_markerTypes
				.Each(marker =>
				{
					strippedNamespace = call
						.HandlerType
						.Namespace
						.Replace(marker.Namespace + ".", string.Empty);
				});

			return strippedNamespace;
		}

		private static string breakUpCamelCaseWithHypen(string input)
		{
			var routeBuilder = new StringBuilder();
			for (var i = 0; i < input.Length; ++i)
			{
				if (i != 0 && char.IsUpper(input[i]))
				{
					routeBuilder.Append("-");
				}

				routeBuilder.Append(input[i]);
			}

			return routeBuilder
				.ToString()
				.ToLower();
		}

		public static bool IsHandlerCall(ActionCall call)
		{
			var isHandler = call.HandlerType.Name.ToLower().EndsWith(HANDLER.ToLower()) ||
							HandlerExpression.IsMatch(call.HandlerType.Name);
			return isHandler && !call.Method.HasAttribute<UrlPatternAttribute>();
		}
	}
}

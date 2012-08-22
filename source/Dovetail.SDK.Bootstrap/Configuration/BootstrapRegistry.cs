using System;
using System.Web;
using Dovetail.SDK.Bootstrap.Authentication;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.History.AssemblerPolicies;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.TemplatePolicies;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Schema;
using StructureMap.Configuration.DSL;

namespace Dovetail.SDK.Bootstrap.Configuration
{
	public interface IClarifySessionFactory
	{
		IClarifySession GetUserSession();
		IApplicationClarifySession GetApplicationSession();
	}

	public class ClarifySessionFactory : IClarifySessionFactory
	{
		private readonly IClarifySessionCache _clarifySessionCache;
		private readonly Func<ICurrentSDKUser> _currentSdkUser;

		public ClarifySessionFactory(IClarifySessionCache clarifySessionCache, Func<ICurrentSDKUser> currentSdkUser)
		{
			_clarifySessionCache = clarifySessionCache;
			_currentSdkUser = currentSdkUser;
		}

		public IClarifySession GetUserSession()
		{
			return _clarifySessionCache.GetSession(_currentSdkUser().Username);
		}

		public IApplicationClarifySession GetApplicationSession()
		{
			return _clarifySessionCache.GetApplicationSession() as IApplicationClarifySession;
		}
	}

	public class BootstrapRegistry : Registry
    {
        public BootstrapRegistry()
        {
            For<ISecurityContext>().Use(c =>
                                            {
                                                if(HttpContext.Current == null)
                                                {
                                                    return c.GetInstance<NullSecurityContext>();
                                                }
                                                
                                                return c.GetInstance<AspNetSecurityContext>();
                                            });

            For<ILogger>()
                .AlwaysUnique()
                .Use(s => s.ParentType == null ? new Log4NetLogger(s.BuildStack.Current.ConcreteType) : new Log4NetLogger(s.ParentType));
            
            Scan(s =>
            {
                s.TheCallingAssembly();
				s.AssemblyContainingType<IClarifySessionCache>();
                s.WithDefaultConventions();
                s.AddAllTypesOf<IHistoryAssemblerPolicy>();
            });

            //IncludeRegistry<AppSettingProviderRegistry>();

            ForSingletonOf<IClarifyApplicationFactory>().Use<ClarifyApplicationFactory>();
			ForSingletonOf<IClarifyApplication>().Use(ctx => ctx.GetInstance<IClarifyApplicationFactory>().Create());

            //configure the container to use the session cache as a factory for the current user's session	
            //any web class that takes a dependency on IClarifySession will get a session for the current 
            //authenticated user. 
            ForSingletonOf<IClarifySessionCache>().Use<ClarifySessionCache>();
			For<IClarifySessionFactory>().Use<ClarifySessionFactory>();
			For<IClarifySession>().HybridHttpOrThreadLocalScoped().Use(ctx=>ctx.GetInstance<IClarifySessionFactory>().GetUserSession());
			For<IApplicationClarifySession>().HybridHttpOrThreadLocalScoped().Use(ctx => ctx.GetInstance<IClarifySessionCache>().GetApplicationSession() as ClarifySessionWrapper);

			//Make Dovetail SDK caches directly available for DI.
			For<IListCache>().Use(c => c.GetInstance<IClarifyApplication>().ListCache);
			For<ISchemaCache>().Use(c => c.GetInstance<IClarifyApplication>().SchemaCache);
			For<IStringCache>().Use(c => c.GetInstance<IClarifyApplication>().StringCache);
			For<ILocaleCache>().Use(c => c.GetInstance<IClarifyApplication>().LocaleCache);
			For<IListCache>().Use(c => c.GetInstance<IClarifyApplication>().ListCache);

            //It is the responsibility of the applicationUrl using bootstrap to set the current sdk user's login 
            For<ICurrentSDKUser>().HybridHttpOrThreadLocalScoped().Use<CurrentSDKUser>();
        	For<IUserClarifySessionConfigurator>().Use<UTCTimezoneUserClarifySessionConfigurator>();

	        For<IWebApplicationUrl>().Use<AspNetWebApplicationUrl>();

            this.ActEntryTemplatePolicies<DefaultActEntryTemplatePolicyRegistry>();
        }
    }
}
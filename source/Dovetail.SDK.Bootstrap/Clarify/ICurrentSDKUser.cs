using System.Security.Principal;
using Dovetail.SDK.Bootstrap.Authentication;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.DataObjects;

namespace Dovetail.SDK.Bootstrap.Clarify
{
    public interface ICurrentSDKUser
    {
    	string Username { get; }
        bool IsAuthenticated { get; }
        bool HasPermission(string permission);
        ITimeZone Timezone { get; }

        void SignOut();
        void SetUser(string username);
        void SetUser(IPrincipal principal);
    }

    public class CurrentSDKUser : ICurrentSDKUser
    {
        private IPrincipal _principal; 
        private readonly DovetailDatabaseSettings _settings;
        private readonly ILogger _logger;
        private readonly IUserDataAccess _userDataAccess;
        private readonly ILocaleCache _localeCache;
        public bool IsAuthenticated { get; private set; }

        public string Username { get; set; }
        public ITimeZone Timezone { get; set; }

        public CurrentSDKUser(DovetailDatabaseSettings settings, ILocaleCache localeCache, ILogger logger, IUserDataAccess userDataAccess)
        {
            _settings = settings;
            _logger = logger;
            _userDataAccess = userDataAccess;
            _localeCache = localeCache;

            //set up defaults
            SignOut();
        }

        public bool HasPermission(string permission)
        {
            return _principal != null && _principal.IsInRole(permission);
        } 
        
        public void SetUser(IPrincipal principal)
        {
            _principal = principal;
            var username = principal.Identity.Name;

            var timezone = _userDataAccess.GetUserSiteTimezone(username); 

            if (timezone == null)
            {
                _logger.LogWarn("Could not find user's site timezone. Setting their timezone to the default timezone.");
                timezone = _localeCache.ServerTimeZone;
            }

            Username = username;
            IsAuthenticated = true;
            Timezone = timezone;
        }

        public void SetUser(string username)
        {
            var principal = new DovetailPrincipal(new GenericIdentity(username), new string[0]);
            SetUser(principal);
        }

        public void SignOut()
        {
            IsAuthenticated = false;

            //when no user is authenticated the application user is used
            Username = _settings.ApplicationUsername;
            Timezone = _localeCache.ServerTimeZone;
        }
    }

}
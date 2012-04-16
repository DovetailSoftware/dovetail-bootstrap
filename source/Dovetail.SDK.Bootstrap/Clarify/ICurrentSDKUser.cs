using System;
using System.Collections.Generic;
using System.Security.Principal;
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
        IEnumerable<UserQueue> Queues { get; }

        void SignOut();
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

        private readonly Lazy<ITimeZone> _timeZone; 
        public ITimeZone Timezone { 
            get
            {
                if (Username == _settings.ApplicationUsername)
                    return _localeCache.ServerTimeZone;

                return _timeZone.Value;
            }
        }

        private readonly Lazy<IEnumerable<UserQueue>> _queues;
        public IEnumerable<UserQueue> Queues {
            get
            {
                if (Username == _settings.ApplicationUsername)
                    return new UserQueue[0];

                return _queues.Value;
            }
        }

        public CurrentSDKUser(DovetailDatabaseSettings settings, ILocaleCache localeCache, ILogger logger, IUserDataAccess userDataAccess)
        {
            _settings = settings;
            _logger = logger;
            _userDataAccess = userDataAccess;
            _localeCache = localeCache;

            //set up defaults
            SignOut();

            _timeZone = new Lazy<ITimeZone>(()=>
            {
                var timezone = _userDataAccess.GetUserSiteTimezone(Username);

                if (timezone == null)
                {
                    _logger.LogWarn("Could not find user {0}'s site timezone. Setting their timezone to the default timezone.", Username);
                    return _localeCache.ServerTimeZone;
                }

                return timezone;
            });

            _queues = new Lazy<IEnumerable<UserQueue>>(() => _userDataAccess.GetQueueMemberships(Username));
        }

        public bool HasPermission(string permission)
        {
            return _principal != null && _principal.IsInRole(permission);
        } 
        
        public void SetUser(IPrincipal principal)
        {
            _principal = principal;
            
            Username = _principal.Identity.Name;
            
            IsAuthenticated = true;
        }

        public void SignOut()
        {
            IsAuthenticated = false;

            //when no user is authenticated the application user is used
            Username = _settings.ApplicationUsername;
        }
    }
}
using System;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify
{
    public interface IClarifySessionProvider
    {
        IClarifySession GetHttpRequestSession();
    }

    public class ClarifySessionProvider : IClarifySessionProvider
    {
        private readonly IClarifySessionCache _sessionCache;
        private readonly ICurrentSDKUser _user;

        public ClarifySessionProvider(IClarifySessionCache sessionCache, ICurrentSDKUser user)
        {
            _sessionCache = sessionCache;
            _user = user;
        }

        public IClarifySession GetHttpRequestSession()
        {
            if(_user.Username.IsEmpty())
            {
                throw new ArgumentException("Clarify session provider needs to know who the current SDK user. It is the responsibility of calling code to set this value.");
            }

            return  (_user.IsContact) ? _sessionCache.GetContactSession(_user.Username) : _sessionCache.GetUserSession(_user.Username);
        }
    }
}
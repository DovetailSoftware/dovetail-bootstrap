using System;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.AuthToken
{
    public interface IAuthenticationTokenRepository
    {
        IAuthenticationToken RetrieveByToken(string token);
        IAuthenticationToken RetrieveByUsername(string username);
        IAuthenticationToken GenerateToken(string username);
    }

    public class AuthenticationTokenRepository : IAuthenticationTokenRepository
    {
        private readonly IClarifySessionCache _sessionCache;
        private readonly ILogger _logger;

        public AuthenticationTokenRepository(IClarifySessionCache sessionCache, ILogger logger)
        {
            _sessionCache = sessionCache;
            _logger = logger;
        }

        public IAuthenticationToken RetrieveByUsername(string username)
        {
            var userGeneric = getQueriedUserGeneric(username);

            var authToken = userGeneric.DataRows().First().AsString("x_authtoken");

            if(authToken.IsNotEmpty())
            {
                _logger.LogDebug("Found token {0} for user {1}.".ToFormat(authToken, username));

                return new AuthenticationToken { Token = authToken, Username = username };
            }

            _logger.LogDebug("No existing token was found for user {1} generating one.".ToFormat(authToken, username));
            return GenerateToken(username);
        }

        public IAuthenticationToken RetrieveByToken(string token)
        {
            var session = _sessionCache.GetApplicationSession();

            var dataSet = session.CreateDataSet();
            var userGeneric = dataSet.CreateGeneric("user");
            userGeneric.DataFields.Add("login_name");
            userGeneric.Filter(f => f.Equals("x_authtoken", token));
            userGeneric.Query();

            if (userGeneric.Count < 1)
            {
                _logger.LogDebug("No user for token {0} was found.".ToFormat(token));
                return null;
            }
            
            var username = userGeneric.DataRows().First().AsString("login_name");
            _logger.LogDebug("Found user {0} for token {1}.".ToFormat(username, token));

            return new AuthenticationToken { Token = token, Username = username };
        }

        public IAuthenticationToken GenerateToken(string username)
        {
            var userGeneric = getQueriedUserGeneric(username);

            var userRow = userGeneric.DataRows().First();

            var newToken = Guid.NewGuid().ToString().Replace("-","");

            _logger.LogInfo("New user authentication token {0} created for {1}.".ToFormat(newToken, username));

            userRow["x_authtoken"] = newToken ;
            userGeneric.UpdateAll();

            return new AuthenticationToken {Token = newToken, Username = username};
        }

        private ClarifyGeneric getQueriedUserGeneric(string username)
        {
            var session = _sessionCache.GetApplicationSession();

            var dataSet = session.CreateDataSet();
            var userGeneric = dataSet.CreateGeneric("user");
            userGeneric.DataFields.Add("x_authtoken");
            userGeneric.Filter(f => f.Equals("login_name", username));
            userGeneric.Query();

            if (userGeneric.Count < 1)
            {
                throw new ApplicationException("No user {0} was found.".ToFormat(username));
            }

            return userGeneric;
        }
    }
}
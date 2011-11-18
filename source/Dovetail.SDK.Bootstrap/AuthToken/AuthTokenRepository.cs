using System;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.AuthToken
{
    public interface IAuthTokenRepository
    {
        string Retrieve(string username);
        string Generate(string username);
    }

    public class AuthTokenRepository : IAuthTokenRepository
    {
        private readonly IClarifySessionCache _sessionCache;
        private readonly ILogger _logger;

        public AuthTokenRepository(IClarifySessionCache sessionCache, ILogger logger)
        {
            _sessionCache = sessionCache;
            _logger = logger;
        }

        public string Retrieve(string username)
        {
            var userGeneric = getQueriedUserGeneric(username);

            var authToken = userGeneric.DataRows().First().AsString("x_authtoken");

            if(authToken.IsNotEmpty())
            {
                _logger.LogDebug("Found token {0} for user {1}.".ToFormat(authToken, username));

                return authToken;
            }

            _logger.LogDebug("No existing token found for user {1} generating one.".ToFormat(authToken, username));
            return Generate(username);
        }

        public string Generate(string username)
        {
            var userGeneric = getQueriedUserGeneric(username);

            var userRow = userGeneric.DataRows().First();

            var newToken = Guid.NewGuid().ToString();

            _logger.LogInfo("New user authentication token {0} created for {1}.".ToFormat(newToken, username));

            userRow["x_authtoken"] = newToken ;
            userGeneric.UpdateAll();

            return newToken;
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
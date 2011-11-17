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
        string Get(string username);
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

        public string Get(string username)
        {
            var userGeneric = getQueriedUserGeneric(username);

            var authToken = userGeneric.DataRows().First().AsString("x_authtoken");

            _logger.LogDebug("Found token {0} for user {1}.".ToFormat(authToken, username));

            return authToken;
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
                _logger.LogDebug("No user {0} was found.".ToFormat(username));
                return userGeneric;
            }
            return userGeneric;
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
    }
}
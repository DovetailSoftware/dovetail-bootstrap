using System;
using System.Net;
using Dovetail.SDK.Bootstrap.Clarify;
using FubuCore;
using FubuCore.Binding;
using FubuMVC.Core;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Runtime;

namespace Dovetail.SDK.Bootstrap.Token
{
    public class AuthenticationTokenBehavior : BasicBehavior
    {
        private readonly IFubuRequest _request;
        private readonly IOutputWriter _outputWriter;
        private readonly IAuthenticationTokenRepository _tokenRepository;
        private readonly AggregateDictionary _aggregateDictionary;
        private readonly ICurrentSDKUser _currentSdkUser;
        private readonly ILogger _logger;

        public AuthenticationTokenBehavior(IFubuRequest request, IOutputWriter outputWriter, IAuthenticationTokenRepository tokenRepository,
            AggregateDictionary aggregateDictionary, ICurrentSDKUser currentSdkUser, ILogger logger) 
            : base(PartialBehavior.Executes)
        {
            _request = request;
            _outputWriter = outputWriter;
            _tokenRepository = tokenRepository;
            _aggregateDictionary = aggregateDictionary;
            _currentSdkUser = currentSdkUser;
            _logger = logger;
        }

        protected override DoNext performInvoke()
        {
            string source = null;
            string token = null;
            _aggregateDictionary.Value("authToken", (s, v) =>
                                                        {
                                                            source = s;
                                                            if(v != null)
                                                            {
                                                                token = Convert.ToString(v);
                                                            }
                                                        });

            if(token.IsEmpty())
            {
                WriteUnauthorizedError("No valid authentication token was found in HTTP headers, querystring, or post parameters.");
                return DoNext.Stop;
            }

            _logger.LogDebug("Authentication token {0} found in {1}.", token, source);

            var authenticationToken = _tokenRepository.RetrieveByToken(token);
            if (authenticationToken == null)
            {
                WriteUnauthorizedError("Authentication token {0} was found in {1} but was not valid for any users.".ToFormat(token, source));
                return DoNext.Stop;

            }

            _logger.LogDebug("Authentication token {0} found in {1} validated for user {2}.", authenticationToken, source, authenticationToken);
            _request.Set(authenticationToken);

        	_currentSdkUser.Username = authenticationToken.Username;

            return DoNext.Continue;    
        }

        private void WriteUnauthorizedError(string message)
        {
            _outputWriter.WriteResponseCode(HttpStatusCode.Unauthorized);
            _outputWriter.WriteHtml(message);
        }
    }

}
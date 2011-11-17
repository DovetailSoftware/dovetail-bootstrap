namespace Dovetail.SDK.Bootstrap.AuthToken
{
    public class TokenAuthenticationApi
    {
        private readonly IAuthTokenRepository _repository;
        private readonly IUserAuthenticator _authenticator;

        public TokenAuthenticationApi(IAuthTokenRepository repository, IUserAuthenticator authenticator)
        {
            _repository = repository;
            _authenticator = authenticator;
        }

        public TokenAuthenticationResult ResetToken(string username, string password)
        {
            var result = new TokenAuthenticationResult {Success = false};

            if (_authenticator.Authenticate(username,password))
            {
                result.Success = true;
                result.Token = _repository.Generate(username);
            }

            return result;
        }

        public TokenAuthenticationResult GetToken(string username, string password)
        {
            var result = new TokenAuthenticationResult { Success = false };

            if (_authenticator.Authenticate(username, password))
            {
                result.Success = true;
                result.Token = _repository.Get(username);
            }

            return result;
        }
    }
}
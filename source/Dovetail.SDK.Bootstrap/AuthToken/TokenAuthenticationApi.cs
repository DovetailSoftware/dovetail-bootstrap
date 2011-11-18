namespace Dovetail.SDK.Bootstrap.AuthToken
{
    public class TokenAuthenticationResetResult : TokenAuthenticationResult { }
    public class TokenAuthenticationResult : IApi
    {
        public string Username { get; set; }
        public bool Success { get; set; }
        public string Token { get; set; }
    }

    public interface ITokenAuthenticationApi
    {
        TokenAuthenticationResult GetToken(string username, string password);
        TokenAuthenticationResetResult ResetToken(string username, string password);
    }

    public class TokenAuthenticationApi : ITokenAuthenticationApi
    {
        private readonly IAuthTokenRepository _repository;
        private readonly IUserAuthenticator _authenticator;

        public TokenAuthenticationApi(IAuthTokenRepository repository, IUserAuthenticator authenticator)
        {
            _repository = repository;
            _authenticator = authenticator;
        }

        public TokenAuthenticationResult GetToken(string username, string password)
        {
            var result = new TokenAuthenticationResult { Success = false, Username = username };

            if (_authenticator.Authenticate(username, password))
            {
                result.Success = true;
                result.Token = _repository.Retrieve(username);
            }

            return result;
        }

        public TokenAuthenticationResetResult ResetToken(string username, string password)
        {
            var result = new TokenAuthenticationResetResult { Success = false, Username = username };

            if (_authenticator.Authenticate(username,password))
            {
                result.Success = true;
                result.Token = _repository.Generate(username);
            }

            return result;
        }
    }
}
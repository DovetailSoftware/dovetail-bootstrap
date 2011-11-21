namespace Dovetail.SDK.Bootstrap.AuthToken
{
    public interface ITokenAuthenticationApi
    {
        IAuthenticationToken GetToken(string username, string password);
        IAuthenticationToken ResetToken(string username, string password);
    }

    public class TokenAuthenticationApi : ITokenAuthenticationApi
    {
        private readonly IAuthenticationTokenRepository _repository;
        private readonly IUserAuthenticator _authenticator;

        public TokenAuthenticationApi(IAuthenticationTokenRepository repository, IUserAuthenticator authenticator)
        {
            _repository = repository;
            _authenticator = authenticator;
        }

        public IAuthenticationToken GetToken(string username, string password)
        {
            if (_authenticator.Authenticate(username, password) == false)
            {
                return new AuthenticationToken { Username = username }; 
            }

            return _repository.RetrieveByUsername(username);
        }

        public IAuthenticationToken ResetToken(string username, string password)
        {
            if (_authenticator.Authenticate(username,password) == false)
            {
                return new AuthenticationToken { Username = username };
            }

            return _repository.GenerateToken(username);
        }
    }
}
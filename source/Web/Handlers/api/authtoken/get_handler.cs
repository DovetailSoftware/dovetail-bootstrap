namespace Bootstrap.Web.Handlers.api.authtoken
{
    public class get_handler
    {
        public AuthTokenModel Execute(AuthTokenRequest request)
        {
            return new AuthTokenModel {};
        }
    }
    
    public class AuthTokenRequest
    {
    }

    public class AuthTokenModel 
    {
    }
}
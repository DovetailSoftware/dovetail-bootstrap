namespace Dovetail.SDK.Bootstrap.AuthToken
{
    public interface IAuthenticationToken
    {
        string Username { get; set; }
        string Token { get; set; }
    }

    public class AuthenticationToken : IAuthenticationToken
    {
        public string Username { get; set; }
        public string Token { get; set; }
    }
}
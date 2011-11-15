namespace Dovetail.SDK.Bootstrap.Clarify
{
    public interface ICurrentSDKUser
    {
        bool IsContact { get; set; }
        string Username { get; set; }
    }

    public class CurrentSDKUser : ICurrentSDKUser
    {
        public bool IsContact { get; set; }
        public string Username { get; set; }
    }
}
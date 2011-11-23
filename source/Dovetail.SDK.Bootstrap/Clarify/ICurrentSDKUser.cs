namespace Dovetail.SDK.Bootstrap.Clarify
{
    public interface ICurrentSDKUser
    {
    	string Username { get; set; }
    }

    public class CurrentSDKUser : ICurrentSDKUser
    {
    	public string Username { get; set; }
    }
}
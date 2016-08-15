namespace Dovetail.SDK.Clarify
{
    public interface IClarifySessionManager
    {
        void Configure(IClarifySession session);
        void Eject(IClarifySession session);
    }
}
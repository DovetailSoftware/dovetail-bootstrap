namespace Dovetail.SDK.Clarify
{
    public interface IClarifySessionListener
    {
        void Created(IClarifySession session);
        void Started(IClarifySession session);
        void Closed(IClarifySession session);
    }
}
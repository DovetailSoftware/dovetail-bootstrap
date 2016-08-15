using System.Linq;
using System.Reflection;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.Clarify.Tests
{
    public static class SessionUtilities
    {
        private static readonly ConstructorInfo Constructor;

        static SessionUtilities()
        {
            Constructor = typeof(ClarifySession)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(_ => _.GetParameters().Length == 0);
        }

        public static ClarifySession CreateEmptySession()
        {
            return (ClarifySession) Constructor.Invoke(null);
        }
    }
}
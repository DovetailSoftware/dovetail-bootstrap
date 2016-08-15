using System;
using FChoice.Foundation.Clarify;
using FChoice.Toolkits.Clarify.FieldOps;
using FChoice.Toolkits.Clarify.Interfaces;
using FChoice.Toolkits.Clarify.Support;

namespace Dovetail.SDK.Clarify
{
    public static class SessionExtensions
    {
        public static ClarifySession AsClarifySession(this IClarifySession session)
        {
            var wrapper = session as IClarifySessionWrapper;
            if (wrapper == null)
                throw new InvalidOperationException(string.Format("Session must implement {0}", typeof(IClarifySessionWrapper).Name));

            return wrapper.Clarify;
        }

        public static SupportToolkit CreateSupportToolkit(this IClarifySession session)
        {
            return new SupportToolkit(session.AsClarifySession());
        }

        public static FieldOpsToolkit CreateFieldOpsToolkit(this IClarifySession session)
        {
            return new FieldOpsToolkit(session.AsClarifySession());
        }

        public static InterfacesToolkit CreateInterfacesToolkit(this IClarifySession session)
        {
            return new InterfacesToolkit(session.AsClarifySession());
        }
    }
}
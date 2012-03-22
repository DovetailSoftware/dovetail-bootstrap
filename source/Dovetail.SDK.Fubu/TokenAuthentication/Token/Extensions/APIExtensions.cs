using System;
using Dovetail.SDK.Bootstrap;
using FubuCore;

namespace Dovetail.SDK.Fubu.TokenAuthentication.Token.Extensions
{
    public static class APIExtensions
    {
        public static bool IsAPIRequest(this Type type)
        {
            return type.CanBeCastTo<IApi>() || type.CanBeCastTo<IUnauthenticatedApi>();
        }

        public static bool IsAuthenticatedAPIRequest(this Type type)
        {
            return type.CanBeCastTo<IApi>();
        }
    }
}
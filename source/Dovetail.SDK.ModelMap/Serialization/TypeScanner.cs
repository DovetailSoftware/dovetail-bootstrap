using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;

namespace Dovetail.SDK.ModelMap.Serialization
{
    public static class TypeScanner
    {
        private static readonly TypePool Pool = TypePool.AppDomainTypes();
        private static Func<Func<Type, bool>, IEnumerable<Type>> _typesMatching;
        private static readonly IList<Type> Types = new List<Type>();

        static TypeScanner()
        {
            Reset();
        }

        public static Type[] TypesMatching(Func<Type, bool> predicate)
        {
            return _typesMatching(predicate).ToArray();
        }

        public static Type[] ConcreteImplementationsOf<T>()
        {
            return _typesMatching(x => x.IsConcreteTypeOf<T>()).ToArray();
        }

        public static Type[] ConcreteImplementationsOf(Type type)
        {
            return _typesMatching(x => x.IsConcreteTypeOf(type)).ToArray();
        }

        public static Type[] TypesClosing(Type templateType)
        {
            return _typesMatching(x => !x.IsOpenGeneric() && x.ImplementsInterfaceTemplate(templateType)).ToArray();
        }

        public static Type[] InterfaceTemplateImplementations(Type templateType)
        {
            return _typesMatching(x => x.ImplementsInterfaceTemplate(templateType)).ToArray();
        }

        // For testing
        public static void AddTypes(params Type[] types)
        {
            Types.Clear();
            Types.AddRange(types);

            _typesMatching = predicate => Types.Where(predicate);
        }

        public static void WithTypes(IEnumerable<Type> types, Action action)
        {
            AddTypes(types.ToArray());

            try
            {
                action();
            }
            finally
            {
                Reset();
            }
        }

        public static void Reset()
        {
            _typesMatching = Pool.TypesMatching;
            Types.Clear();
        }

        public static bool IsConcreteTypeOf(this Type pluggedType, Type type)
        {
            if (pluggedType == null || !pluggedType.IsConcrete())
            {
                return false;
            }

            return type.IsAssignableFrom(pluggedType);
        }
    }
}
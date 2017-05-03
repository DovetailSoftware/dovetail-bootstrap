using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization
{
    public class TypePool
    {
        private static readonly Lazy<TypePool> _appDomainTypes = new Lazy<TypePool>(() => {
            var pool = new TypePool { IgnoreExportTypeFailures = true };
            pool.AddAssemblies(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic));

            return pool;
        });

        public static TypePool AppDomainTypes()
        {
            return _appDomainTypes.Value;
        }

        private readonly List<Assembly> _assemblies = new List<Assembly>();
        private readonly IList<Type> _types = new List<Type>();
        private bool _scanned;

        public TypePool()
        {
            IgnoreExportTypeFailures = true;
        }

        public bool IgnoreExportTypeFailures { get; set; }


        private IEnumerable<Type> types
        {
            get
            {
                if (!_scanned)
                {
                    _scanned = true;

                    _types.AddRange(Assemblies.Where(x => !x.IsDynamic).SelectMany(x =>
                    {
                        try
                        {
                            return x.GetExportedTypes();
                        }
                        catch (Exception ex)
                        {
                            if (IgnoreExportTypeFailures)
                            {
                                return new Type[0];
                            }
                            else
                            {
                                throw new ApplicationException("Unable to find exported types from assembly " + x.FullName, ex);
                            }
                        }
                    }));
                }


                return _types;
            }
        }


        public void AddAssembly(Assembly assembly)
        {
            _assemblies.Add(assembly);
        }

        public IEnumerable<Assembly> Assemblies
        {
            get
            {
                return _assemblies;
            }
        }

        public IEnumerable<Type> TypesMatching(Func<Type, bool> filter)
        {
            // TODO -- diagnostics on type discovery!!!!
            return types.Where(filter).Distinct();
        }

        public IEnumerable<Type> TypesWithFullName(string fullName)
        {
            return TypesMatching(t => t.FullName == fullName);
        }

        public bool HasAssembly(Assembly assembly)
        {
            return _assemblies.Contains(assembly);
        }

        public void AddAssemblies(IEnumerable<Assembly> assemblies)
        {
            _assemblies.AddRange(assemblies);
        }
    }
}
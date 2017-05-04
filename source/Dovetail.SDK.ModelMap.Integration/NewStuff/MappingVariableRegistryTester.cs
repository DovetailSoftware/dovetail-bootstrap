using System.Collections.Generic;
using Dovetail.SDK.ModelMap.NewStuff;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff
{
    [TestFixture]
    public class MappingVariableRegistryTester
    {
        private List<IMappingVariableSource> theSources;
        private MappingVariableRegistry theRegistry;

        [SetUp]
        public void SetUp()
        {
            theSources = new List<IMappingVariableSource>();
            theRegistry = new MappingVariableRegistry(theSources);
        }

        [Test]
        public void finds_the_last_matching_variable()
        {
            var v1 = new StubVariable("a");
            var v2 = new StubVariable("b");
            var v3 = new StubVariable("c");
            var v4 = new StubVariable("b");

            theSources.Add(new StubVariableSource
            {
                TheVariables =
                {
                    v1,
                    v2
                }
            });

            theSources.Add(new StubVariableSource
            {
                TheVariables =
                {
                    v3,
                    v4
                }
            });

            theRegistry.Find("b").ShouldEqual(v4);
        }

        [Test]
        public void null_when_nothing_matches()
        {
            var v1 = new StubVariable("a");
            var v2 = new StubVariable("b");
            var v3 = new StubVariable("c");
            var v4 = new StubVariable("b");

            theSources.Add(new StubVariableSource
            {
                TheVariables =
                {
                    v1,
                    v2
                }
            });

            theSources.Add(new StubVariableSource
            {
                TheVariables =
                {
                    v3,
                    v4
                }
            });

            theRegistry.Find("d").ShouldBeNull();
        }

        private class StubVariable : IMappingVariable
        {
            public StubVariable(string key)
            {
                Key = key;
            }

            public string Key;

            public bool Matches(string key)
            {
                return key.EqualsIgnoreCase(Key);
            }

            public object Expand(string key, IServiceLocator services)
            {
                throw new System.NotImplementedException();
            }
        }

        private class StubVariableSource : IMappingVariableSource
        {
            public List<IMappingVariable> TheVariables = new List<IMappingVariable>();

            public IEnumerable<IMappingVariable> Variables()
            {
                return TheVariables;
            }
        }
    }
}
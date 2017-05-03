using System.Collections.Generic;
using Dovetail.SDK.ModelMap.NewStuff.Serialization;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff.Serialization
{
    [TestFixture]
    public class ObjectBuilderTester
    {
        [Test]
        public void happy_path_through_constructor()
        {
            var values = new Dictionary<string, string>
            {
                { "a", "1"},
                { "b", "2" }
            };

            var builder = new ObjectBuilder();
            var result = builder.Build(new BuildObjectContext(typeof(ConstructorExample), values));

            result.HasErrors().ShouldBeFalse();
            var example = result.Result.As<ConstructorExample>();

            example.A.ShouldEqual("1");
            example.B.ShouldEqual("2");
        }

        [Test]
        public void missing_constructor_value()
        {
            var values = new Dictionary<string, string>
            {
                { "a", "1"}
            };

            var builder = new ObjectBuilder();
            var result = builder.Build(new BuildObjectContext(typeof(ConstructorExample), values));

            result.HasErrors().ShouldBeTrue();
            result.Result.ShouldBeNull();
        }

        [Test]
        public void happy_path_through_default_constructor_and_properties()
        {
            var values = new Dictionary<string, string>
            {
                { "a", "1"},
                { "b", "2" }
            };

            var builder = new ObjectBuilder();
            var result = builder.Build(new BuildObjectContext(typeof(PropertyExample), values));

            result.HasErrors().ShouldBeFalse();
            var example = result.Result.As<PropertyExample>();

            example.A.ShouldEqual("1");
            example.B.ShouldEqual("2");
        }

        [Test]
        public void missing_properties_has_no_errors()
        {
            var values = new Dictionary<string, string>
            {
                { "b", "2" }
            };

            var builder = new ObjectBuilder();
            var result = builder.Build(new BuildObjectContext(typeof(PropertyExample), values));

            result.HasErrors().ShouldBeFalse();
            var example = result.Result.As<PropertyExample>();

            example.A.ShouldBeNull();
            example.B.ShouldEqual("2");
        }


        private class ConstructorExample
        {
            public ConstructorExample(string a, string b)
            {
                A = a;
                B = b;
            }

            public string A { get; set; }
            public string B { get; set; }
        }

        private class PropertyExample
        {
            public string A { get; set; }
            public string B { get; set; }
        }
    }
}
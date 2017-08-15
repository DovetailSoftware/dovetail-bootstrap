using System.Collections.Generic;
using Dovetail.SDK.ModelMap.Serialization;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.Serialization
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
		public void happy_path_through_constructor_with_string_params()
		{
			var values = new Dictionary<string, string>
			{
				{ "a", "1"},
				{ "param0", "a" },
				{ "param3", "d" },
				{ "param2", "c" },
				{ "param1", "b" },
			};

			var builder = new ObjectBuilder();
			var result = builder.Build(new BuildObjectContext(typeof(ConstructorWithStringParamsExample), values));

			result.HasErrors().ShouldBeFalse();
			var example = result.Result.As<ConstructorWithStringParamsExample>();

			example.A.ShouldEqual("1");
			example.Values.ShouldEqual(new[] { "a", "b", "c", "d"});
		}

		[Test]
		public void happy_path_through_constructor_with_int_params()
		{
			var values = new Dictionary<string, string>
			{
				{ "a", "1"},
				{ "param0", "3" },
				{ "param3", "6" },
				{ "param2", "5" },
				{ "param1", "4" },
			};

			var builder = new ObjectBuilder();
			var result = builder.Build(new BuildObjectContext(typeof(ConstructorWithIntParamsExample), values));

			result.HasErrors().ShouldBeFalse();
			var example = result.Result.As<ConstructorWithIntParamsExample>();

			example.A.ShouldEqual("1");
			example.Values.ShouldEqual(new[] { 3, 4, 5, 6 });
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

	    private class ConstructorWithStringParamsExample
	    {
		    public ConstructorWithStringParamsExample(string a, params string[] values)
		    {
			    A = a;
			    Values = values;
		    }

		    public string A { get; set; }
			public string[] Values { get; set; }
	    }

		private class ConstructorWithIntParamsExample
		{
			public ConstructorWithIntParamsExample(string a, params int[] values)
			{
				A = a;
				Values = values;
			}

			public string A { get; set; }
			public int[] Values { get; set; }
		}

		private class PropertyExample
        {
            public string A { get; set; }
            public string B { get; set; }
        }
    }
}
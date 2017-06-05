using System.Collections.Generic;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration
{
    [TestFixture]
    public class MappingVariableExpanderTester
    {
        private MappingVariableExpander theExpander;
        private StubVariableRegistry theRegistry;

        [SetUp]
        public void SetUp()
        {
            theRegistry = new StubVariableRegistry();
            theExpander = new MappingVariableExpander(theRegistry, new InMemoryServiceLocator());
        }

        [Test]
        public void variable_matches()
        {
            verifyVariable("${test}", true);
            verifyVariable("${test_underscore}", true);
            verifyVariable("${testNumbers13241234}", true);
            verifyVariable("${test.the.dots}", true);
            verifyVariable("${test-the-hyphen}", true);


            verifyVariable("{test}", false);
            verifyVariable("${test", false);
            verifyVariable("$test}", false);
        }

        [Test]
        public void expands_the_variable()
        {
            var variable = new StubVariable {Value = "bar"};
            theRegistry.Variable = variable;

            theExpander.Expand("${foo}").ShouldEqual("bar");
        }

		[Test]
	    public void expands_the_variable_from_the_context()
	    {
			var variable = new StubVariable { Value = "bar" };
			theRegistry.Variable = variable;

			var data = new ModelData();
			var values = new Dictionary<string, object>
			{
				{ "foo", "barbarbar" }
			};
			var context = new VariableExpanderContext(data, values);

			theExpander.PushContext(context);

			theExpander.Expand("${foo}").ShouldEqual("barbarbar");
	    }

        [Test]
        public void returns_the_value_if_the_variable_cannot_be_found()
        {
            theExpander.Expand("${foo}").ShouldEqual("${foo}");
        }

        private void verifyVariable(string value, bool isVariable)
        {
            theExpander.IsVariable(value).ShouldEqual(isVariable);
        }

        private class StubVariable : IMappingVariable
        {
            public bool Matches(VariableExpansionContext context)
            {
                throw new System.NotImplementedException();
            }

            public string Value;

            public object Expand(VariableExpansionContext context)
            {
                return Value;
            }
        }

        private class StubVariableRegistry : IMappingVariableRegistry
        {
            public IMappingVariable Variable;

            public IMappingVariable Find(VariableExpansionContext context)
            {
                return Variable;
            }
        }
    }
}
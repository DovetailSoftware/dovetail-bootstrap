using System.Collections.Generic;
using Dovetail.SDK.ModelMap.NewStuff;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff
{
	[TestFixture]
	public class VariableExpanderContextTester
	{
		private ModelData theData;
		private VariableExpanderContext theContext;
		private Dictionary<string, object> theValues;

		[SetUp]
		public void SetUp()
		{
			theData = new ModelData();
			theValues = new Dictionary<string, object>();
			theContext = new VariableExpanderContext(theData, theValues);
		}

		[Test]
		public void has_key()
		{
			theValues.Add("a", 1);
			theContext.Has("a").ShouldBeTrue();
			theContext.Has("z").ShouldBeFalse();
		}

		[Test]
		public void gets_value()
		{
			theValues.Add("a", 1);
			theContext.Get("a").ShouldEqual(1);
		}
	}
}
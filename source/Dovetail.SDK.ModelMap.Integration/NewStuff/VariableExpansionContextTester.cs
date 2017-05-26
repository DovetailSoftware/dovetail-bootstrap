using Dovetail.SDK.ModelMap.NewStuff;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff
{
	[TestFixture]
	public class VariableExpansionContextTester
	{
		[Test]
		public void matches_key()
		{
			var context = new VariableExpansionContext(new InMemoryServiceLocator(), "foo", null);
			context.Matches("foo").ShouldBeTrue();
			context.Matches("FOO").ShouldBeTrue();


			context.Matches("bar").ShouldBeFalse();
		}
	}
}
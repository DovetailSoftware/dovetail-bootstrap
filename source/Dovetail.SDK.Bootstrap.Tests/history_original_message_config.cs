using System.Collections.Specialized;
using System.Linq;
using Dovetail.SDK.Bootstrap.History.Configuration;
using NUnit.Framework;
using Rhino.Mocks;

namespace Dovetail.SDK.Bootstrap.Tests
{
	[TestFixture]
	public class history_original_message_config : Context<HistoryOriginalMessageConfiguration>
	{
		public override void OverrideMocks()
		{
			_services.PartialMockTheClassUnderTest();
		}

		[Test]
		public void when_no_config_section_is_present_use_default_expressions()
		{
			_cut.Stub(s => s.getConfiguationSection()).Return(null);

			_cut.Expressions.ShouldMatch(HistoryOriginalMessageConfiguration.DefaultOriginalMessageExpressions);
		}

		[Test]
		public void when_config_section_is_present_use_section_expressions()
		{
			var configExpressions = new NameValueCollection {{"1", "first"}, {"2", "second"}, {"3", "third"}};

			_cut.Stub(s => s.getConfiguationSection()).Return(configExpressions);

			var results = _cut.Expressions.ToList();

			results.Count().ShouldEqual(3);
			results[0].IsMatch(configExpressions["1"]).ShouldBeTrue();
			results[1].IsMatch(configExpressions["2"]).ShouldBeTrue();
			results[2].IsMatch(configExpressions["3"]).ShouldBeTrue();
		}

		[Test]
		public void when_config_section_is_not_present_should_log_details()
		{
			_cut.Stub(s => s.getConfiguationSection()).Return(null);

			var results = _cut.Expressions.ToList();

			MockFor<ILogger>().AssertWasCalled(a => a.LogDebug(Arg.Text.Contains("Using default original message expressions.")));
		}

		[Test]
		public void should_log_expressions()
		{
			var configExpressions = new NameValueCollection { { "1", "first" }, { "2", "second" }};

			_cut.Stub(s => s.getConfiguationSection()).Return(configExpressions);
			var results = _cut.Expressions.ToList();

			MockFor<ILogger>().AssertWasCalled(a => a.LogDebug(Arg.Text.Contains("Found 2 regular expressions")));
		}
	}
}
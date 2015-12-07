using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;
using Dovetail.SDK.Bootstrap.Tests.History.Suites;
using NUnit.Framework;
using Rhino.Mocks;
using StructureMap.AutoMocking;

namespace Dovetail.SDK.Bootstrap.Tests.History
{
	[TestFixture]
	public class HistoryParsingScenario
	{
		[Test]
		public void run_suites()
		{
			var @namespace = typeof (SuiteMarker).Namespace;
			var resources = GetType()
				.Assembly
				.GetManifestResourceNames()
				.Where(_ => _.StartsWith(@namespace) && !_.Contains(".output"))
				.Select(_ => new
				{
					Suite = _.Replace(@namespace + ".", ""),
					FullName = _
				});

			foreach (var resource in resources)
			{
				runSuite(resource.Suite, resource.FullName);
			}
		}

		private static void runSuite(string suite, string fullName)
		{
			Debug.WriteLine("Running History Parsing suite: " + suite);

			var outputParser = buildOutputParser();
			var input = readStream(fullName);
			var expected = readStream(fullName.Replace(".txt", ".output.txt")).TrimEnd(Environment.NewLine.ToCharArray());
			string output = "";

			if (fullName.Contains("email"))
			{
				output = outputParser.EncodeEmailLog(input);
			}
			else
			{
				output = HttpUtility.HtmlEncode(input);
			}

			string renderedOutput = new JavaScriptSerializer().Serialize(output);
			renderedOutput.Trim('"').ShouldEqual(expected);
		}

		private static HistoryParsers buildParser()
		{
			var services = new RhinoAutoMocker<HistoryParsers>(MockMode.AAA);
			services.Inject(new HistorySettings());
			services.Inject(new HistoryOriginalMessageConfiguration(services.Get<ILogger>()));
			return services.ClassUnderTest;
		}

		private static HistoryOutputParser buildOutputParser()
		{
			var parser = buildParser();
			return new HistoryOutputParser(new ParagraphEndLocator(),
				new ParagraphAggregator(),
				new HistoryItemParser(parser, MockRepository.GenerateStub<ILogger>()),
				new HistoryItemHtmlRenderer(),
				new HtmlEncodeOutputEncoder(),
				new UrlLinkifier());
		}

		private static string readStream(string resource)
		{
			var assembly = typeof (HistoryParsingScenario).Assembly;
			using (var reader = new StreamReader(assembly.GetManifestResourceStream(resource)))
			{
				return reader.ReadToEnd();
			}
		}
	}
}

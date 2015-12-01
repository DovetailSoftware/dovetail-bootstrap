using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.Parser;
using Dovetail.SDK.Bootstrap.Tests.History.Suites;
using FubuCore;
using NUnit.Framework;
using Sprache;
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

			var parser = buildParser();
			var input = readStream(fullName);
			var expected = readStream(fullName.Replace(".txt", ".output.txt"));

			IEnumerable<IItem> contents;
			if (fullName.Contains("email"))
			{
				contents = parser.ContentItem.Many().End().Parse(input);
			}
			else
			{
				contents = parser.ContentItem.Many().End().Parse(input);
			}

			var actual = new StringBuilder();
			contents.Each(_ =>
			{
				if (_ is IRenderHtml)
				{
					actual.Append(_.As<IRenderHtml>().RenderHtml());
					return;
				}

				actual.Append(_.ToString());
			});

			actual.ToString().ShouldEqual(expected);
		}

		private static HistoryParsers buildParser()
		{
			var services = new RhinoAutoMocker<HistoryParsers>(MockMode.AAA);
			services.Inject(new HistorySettings());
			services.Inject(new HistoryOriginalMessageConfiguration(services.Get<ILogger>()));
			return services.ClassUnderTest;
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

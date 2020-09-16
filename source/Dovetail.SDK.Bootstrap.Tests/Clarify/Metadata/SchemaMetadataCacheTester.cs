using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dovetail.SDK.Bootstrap.Clarify.Metadata;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.Bootstrap.Tests.Clarify.Metadata
{
	[TestFixture]
	public class SchemaMetadataCacheTester
	{
		[Test]
		public void parses_the_field()
		{
			var settings = new SchemaMetadataSettings
			{
				Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Clarify", "Metadata", SchemaMetadataSettings.FileName)
			};

			var visitors = new List<IXElementVisitor> {new ParseTables(), new ParseFields()};
			var service = new XElementService(visitors);
			var services = new InMemoryServiceLocator();
			services.Add<IXElementService>(service);
			services.Add<IXElementSerializer>(new XElementSerializer());

			var cache = new SchemaMetadataCache(settings, new NulloLogger(), service, services);

			// Other tables shouldn't exist but will never be null
			cache.MetadataFor("test").ShouldNotBeNull();

			var table = cache.MetadataFor("case");

			// Other fields shouldn't exist but will also never be null
			table.MetadataFor("test").ShouldNotBeNull();
			var field = table.MetadataFor("creation_time");
			field.IsDateOnlyField().ShouldBeTrue();
		}

		[Test]
		public void race_condition()
		{
			var settings = new SchemaMetadataSettings
			{
				Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Clarify", "Metadata", SchemaMetadataSettings.FileName)
			};

			var visitors = new List<IXElementVisitor> {new ParseTables(), new ParseFields()};
			var service = new XElementService(visitors);
			var services = new InMemoryServiceLocator();
			services.Add<IXElementService>(service);
			services.Add<IXElementSerializer>(new XElementSerializer());

			var cache = new SchemaMetadataCache(settings, new NulloLogger(), service, services);

			// Other tables shouldn't exist but will never be null
			cache.MetadataFor("test").ShouldNotBeNull();

			const string targetTable = "TEST_TABLE";
			var actions = new List<Action>();
			var cleanups = new List<Action>();

			for (var i = 0; i < 500; i++)
			{
				var thread = new Thread(() =>
				{
					cache.MetadataFor("case1");
					cache.MetadataFor("case1");
					cache.MetadataFor(targetTable);
					cache.MetadataFor(targetTable);
					cache.MetadataFor(targetTable);
					cache.MetadataFor(targetTable);
				});

				actions.Add(() =>
				{
					Thread.Sleep(50);
					thread.Start();
				});

				cleanups.Add(() => thread.Join());
			}

			Parallel.Invoke(actions.ToArray());

			cleanups.Each(callback => callback());

			cache.MetadataFor(targetTable).ShouldNotBeNull();
		}
	}
}

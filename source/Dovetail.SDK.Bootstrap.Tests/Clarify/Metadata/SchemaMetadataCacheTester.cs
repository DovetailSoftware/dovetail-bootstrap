using System;
using System.Collections.Generic;
using System.IO;
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
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public class SchemaMetadataCache : ISchemaMetadataCache
	{
		private readonly SchemaMetadataSettings _settings;
		private readonly ILogger _logger;
		private readonly IXElementService _elements;
		private readonly IServiceLocator _services;
		private readonly Lazy<List<TableSchemaMetadata>> _metadata;

		public SchemaMetadataCache(SchemaMetadataSettings settings, ILogger logger, IXElementService elements, IServiceLocator services)
		{
			_settings = settings;
			_logger = logger;
			_elements = elements;
			_services = services;
			_metadata = new Lazy<List<TableSchemaMetadata>>(parseMetadata);
		}

		public TableSchemaMetadata MetadataFor(string tableName)
		{
			var table = _metadata.Value.SingleOrDefault(_ => _.Name.EqualsIgnoreCase(tableName));
			if (table == null)
			{
				table = new TableSchemaMetadata
				{
					Name = tableName
				};

				_metadata.Value.Add(table);
			}

			return table;
		}

		private List<TableSchemaMetadata> parseMetadata()
		{
			using (var reader = new StreamReader(_settings.Path))
			{
				var doc = XDocument.Load(reader);

				try
				{
					return parse(doc);
				}
				catch (Exception exc)
				{
					_logger.LogError(exc.Message, exc);
					throw;
				}
			}
		}

		private List<TableSchemaMetadata> parse(XDocument document)
		{
			var root = document.Root;
			var context = new ParsingContext(_services);
			var metadata = new List<TableSchemaMetadata>();
			context.PushObject(metadata);

			root
				.Elements()
				.Each(_ => _elements.Visit(_, context));

			context.PopObject();

			return metadata;
		}
	}
}

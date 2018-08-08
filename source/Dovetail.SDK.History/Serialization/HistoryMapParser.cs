using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Instrumentation;
using System.Xml.Linq;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Instructions;
using Dovetail.SDK.ModelMap.Serialization;
using FubuCore;

namespace Dovetail.SDK.History.Serialization
{
	public class HistoryMapParser : IHistoryMapParser
	{
		private readonly IServiceLocator _services;
		private readonly IXElementService _elementService;
		private readonly ILogger _logger;

		public HistoryMapParser(IServiceLocator services, IXElementService elementService, ILogger logger)
		{
			_services = services;
			_elementService = elementService;
			_logger = logger;
		}

		public ModelMap.ModelMap Parse(string filePath)
		{
			_logger.LogInfo("Parsing model map config at: " + filePath);

			using (var reader = new StreamReader(filePath))
			{
				var doc = XDocument.Load(reader);
				var report = new ModelMapCompilationReport();

				ModelMap.ModelMap config;
				try
				{
					config = Parse(doc, report, filePath.EndsWith(".partial.config"));
				}
				catch (Exception exc)
				{
					report.AddError(exc.Message);
					report.ReportTo(_logger);

					throw;
				}

				return config;
			}
		}

		public void Parse(ModelMap.ModelMap map, string filePath)
		{
			_logger.LogInfo("Parsing model map config at: " + filePath);
			using (var reader = new StreamReader(filePath))
			{
				var doc = XDocument.Load(reader);
				var report = new ModelMapCompilationReport();

				try
				{
					parse(map, doc, report, false);
				}
				catch (Exception exc)
				{
					report.AddError(exc.Message);
					report.ReportTo(_logger);

					throw;
				}
			}
		}

		public ModelMap.ModelMap Parse(XDocument document, ModelMapCompilationReport report, bool isPartial)
		{
			var root = document.Root;
			var name = "";
			var entity = "";

			if (isPartial)
			{
				name = root.Attribute("name").Value;
			}
			else
			{
				var objectType = root.Attribute("objectType");
				if (objectType == null)
					throw new InvalidOperationException("No objectType specified");

				name = WorkflowObject.KeyFor(objectType.Value);
				entity = objectType.Value;
			}


			var map = new ModelMap.ModelMap(name, entity);
			parse(map, document, report, !isPartial);

			return map;
		}

		private void parse(ModelMap.ModelMap map, XDocument document, ModelMapCompilationReport report, bool shouldAddDefaults)
		{
			var root = document.Root;
			var context = new ParsingContext(_services, report);

			map.AddInstruction(new BeginModelMap(map.Name));
			context.PushObject(map);

			if (shouldAddDefaults)
				addDefaults(map);

			root
				.Elements()
				.Each(_ => _elementService.Visit(_, map, context));

			context.PopObject();
			map.AddInstruction(new EndModelMap());
		}

		private void addDefaults(ModelMap.ModelMap map)
		{
			map.AddInstruction(new BeginProperty
			{
				Key = new DynamicValue("id"),
				DataType = new DynamicValue("int"),
				Field = new DynamicValue("objid")
			});
			map.AddInstruction(new EndProperty());

			map.AddInstruction(new BeginProperty
			{
				Key = new DynamicValue("type"),
				DataType = new DynamicValue("int"),
				Field = new DynamicValue("act_code")
			});
			map.AddInstruction(new EndProperty());

			map.AddInstruction(new BeginProperty
			{
				Key = new DynamicValue("timestamp"),
				DataType = new DynamicValue("dateTime"),
				Field = new DynamicValue("entry_time")
			});
			map.AddInstruction(new EndProperty());

			addPerformedBy(map);
			addImpersonatedBy(map);
		}

		private void addPerformedBy(ModelMap.ModelMap map)
		{
			map.AddInstruction(new BeginMappedProperty
			{
				Key = new DynamicValue("performedBy"),
			});
			map.AddInstruction(new BeginRelation
			{
				RelationName = new DynamicValue("act_entry2user"),
			});
			map.AddInstruction(new BeginProperty
			{
				Key = new DynamicValue("username"),
				DataType = new DynamicValue("string"),
				Field = new DynamicValue("login_name")
			});
			map.AddInstruction(new EndProperty());


			map.AddInstruction(new BeginRelation
			{
				RelationName = new DynamicValue("user2employee"),
			});
			map.AddInstruction(new BeginProperty
			{
				Key = new DynamicValue("firstName"),
				DataType = new DynamicValue("string"),
				Field = new DynamicValue("first_name")
			});
			map.AddInstruction(new EndProperty());
			map.AddInstruction(new BeginProperty
			{
				Key = new DynamicValue("lastName"),
				DataType = new DynamicValue("string"),
				Field = new DynamicValue("last_name")
			});
			map.AddInstruction(new EndProperty());
			map.AddInstruction(new EndRelation());
			map.AddInstruction(new EndRelation());

			map.AddInstruction(new EndMappedProperty());
		}

		private void addImpersonatedBy(ModelMap.ModelMap map)
		{
			map.AddInstruction(new BeginMappedProperty
			{
				Key = new DynamicValue("impersonatedBy"),
			});
			map.AddInstruction(new BeginAdHocRelation
			{
				FromTableField = new DynamicValue("proxy"),
				ToTableName = new DynamicValue("empl_user"),
				ToTableFieldName = new DynamicValue("login_name"),
			});

			map.AddInstruction(new BeginProperty
			{
				Key = new DynamicValue("id"),
				DataType = new DynamicValue("int"),
				Field = new DynamicValue("employee")
			});
			map.AddInstruction(new EndProperty());

			map.AddInstruction(new BeginProperty
			{
				Key = new DynamicValue("username"),
				DataType = new DynamicValue("string"),
				Field = new DynamicValue("login_name")
			});
			map.AddInstruction(new EndProperty());

			map.AddInstruction(new BeginProperty
			{
				Key = new DynamicValue("firstName"),
				DataType = new DynamicValue("string"),
				Field = new DynamicValue("first_name")
			});
			map.AddInstruction(new EndProperty());

			map.AddInstruction(new BeginProperty
			{
				Key = new DynamicValue("lastName"),
				DataType = new DynamicValue("string"),
				Field = new DynamicValue("last_name")
			});
			map.AddInstruction(new EndProperty());
			map.AddInstruction(new EndRelation());

			map.AddInstruction(new EndMappedProperty());
		}
	}
}

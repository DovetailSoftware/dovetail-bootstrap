using System;
using Dovetail.SDK.ModelMap.Instructions;
using FChoice.Foundation.Clarify.Schema;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.ModelMap
{
	public class ModelInspectorVisitor : IModelMapVisitor
	{
		private ISchemaTableBase _schemaTable;
		private readonly ISchemaCache _schemaCache;
		private readonly IServiceLocator _services;

		public ModelInspectorVisitor(ISchemaCache schemaCache, IServiceLocator services)
		{
			_schemaCache = schemaCache;
			_services = services;
		}

		public ModelMapProperty Identifier { get; set; }

		public void Visit(BeginModelMap instruction)
		{
			Identifier = null;
		}

		public void Visit(BeginTable instruction)
		{
			_schemaTable = _schemaCache.Tables[instruction.TableName];

			if (_schemaTable == null)
			{
				throw new Exception("No table {0} was found in the schema.".ToFormat(instruction.TableName));
			}
		}

		public void Visit(BeginView instruction)
		{
			_schemaTable = _schemaCache.Views[instruction.ViewName];

			if (_schemaTable == null)
			{
				throw new Exception("No view {0} was found in the schema.".ToFormat(instruction.ViewName));
			}
		}

		public void Visit(BeginProperty instruction)
		{
			if (!instruction.IsIdentifier) return;

			if (Identifier != null)
				throw new ArgumentException("There are multiple identifying fields defined on the model map being visited.");

			var field = instruction.Field.Resolve(_services).ToString();
			Identifier = new ModelMapProperty
			{
				FieldName = field,
				Key = instruction.Key.Resolve(_services).ToString(),
				SchemaFieldType = getSchemaFieldType(field)
			};
		}

		private Type getSchemaFieldType(string field)
		{
			_schemaCache.IsValidField(_schemaTable.Name, field);

			var schemaField = _schemaTable.Fields[field];

			var isString = schemaField.DataType == (int)SchemaCommonType.String;

			return isString ? typeof(string) : typeof(int);
		}

		#region No-op
		public void Visit(EndTable instruction)
		{
		}

		public void Visit(EndView instruction)
		{
		}

		public void Visit(EndModelMap instruction)
		{
		}

		public void Visit(EndProperty instruction)
		{
		}

		public void Visit(BeginAdHocRelation instruction)
		{
		}

		public void Visit(BeginRelation instruction)
		{
		}

		public void Visit(EndRelation instruction)
		{
		}

		public void Visit(BeginMappedProperty instruction)
		{
		}

		public void Visit(EndMappedProperty instruction)
		{
		}

		public void Visit(BeginMappedCollection instruction)
		{
		}

		public void Visit(EndMappedCollection instruction)
		{
		}

		public void Visit(FieldSortMap instruction)
		{
		}

		public void Visit(AddFilter instruction)
		{
		}

		public void Visit(BeginTransform instruction)
		{
		}

		public void Visit(EndTransform instruction)
		{
		}

		public void Visit(AddTransformArgument instruction)
		{
		}

		public void Visit(RemoveProperty instruction)
		{
		}

		public void Visit(RemoveMappedProperty instruction)
		{
		}

		public void Visit(RemoveMappedCollection instruction)
		{
		}

		public void Visit(AddTag instruction)
		{
		}

		public void Visit(PushVariableContext instruction)
		{
		}

		public void Visit(PopVariableContext instruction)
		{
		}
		#endregion
	}
}
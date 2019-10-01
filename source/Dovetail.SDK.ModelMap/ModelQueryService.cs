using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dovetail.SDK.Bootstrap.Clarify.Metadata;
using Dovetail.SDK.ModelMap.Instructions;
using FChoice.Foundation.Schema;
using FubuCore;
using FubuCore.Reflection;
using FubuCore.Util;

namespace Dovetail.SDK.ModelMap
{
	public interface IModelQueryService
	{
		ModelQueryResult Query<TEntity>();
	}

	public class ModelQueryService : IModelQueryService
	{
		private readonly ISchemaCache _schema;
		private readonly ISchemaMetadataCache _metadata;
		private readonly ITypeDescriptorCache _typeDescriptor;
		private readonly IServiceLocator _services;
		private readonly IModelMapRegistry _models;
		private readonly Cache<Type, ModelQueryResult> _results;

		public ModelQueryService(ISchemaCache schema, ISchemaMetadataCache metadata, ITypeDescriptorCache typeDescriptor, IServiceLocator services, IModelMapRegistry models)
		{
			_schema = schema;
			_metadata = metadata;
			_typeDescriptor = typeDescriptor;
			_services = services;
			_models = models;

			_results = new Cache<Type, ModelQueryResult>(query);
		}

		public ModelQueryResult Query<TEntity>()
		{
			return _results[typeof(TEntity)];
		}

		private ModelQueryResult query(Type entityType)
		{
			var map = _models.Find(entityType.Name.ToLower());
			if (map == null)
				return null;

			var properties = _typeDescriptor.GetPropertiesFor(entityType).Values.ToArray();
			var visitor = new ModelQueryVisitor(_schema, _metadata, properties, _services);

			map.Accept(visitor);

			return new ModelQueryResult(visitor.Table, visitor.Properties);
		}

		private class ModelQueryVisitor : IModelMapVisitor
		{
			private readonly ISchemaCache _schema;
			private readonly ISchemaMetadataCache _metadata;
			private readonly PropertyInfo[] _properties;
			private readonly IServiceLocator _services;

			public ModelQueryVisitor(ISchemaCache schema, ISchemaMetadataCache metadata, PropertyInfo[] properties, IServiceLocator services)
			{
				_schema = schema;
				_metadata = metadata;
				_properties = properties;
				_services = services;

				Properties = new List<MappedProperty>();
			}

			public ISchemaTableBase Table;
			public List<MappedProperty> Properties;

			public void Visit(BeginModelMap instruction)
			{
			}

			public void Visit(BeginTable instruction)
			{
				if (Table != null) return;
				Table = _schema.Tables[instruction.TableName];
			}

			public void Visit(EndTable instruction)
			{
			}

			public void Visit(BeginView instruction)
			{
				if (Table != null) return;
				Table = _schema.Views[instruction.ViewName];
			}

			public void Visit(EndView instruction)
			{
			}

			public void Visit(EndModelMap instruction)
			{
			}

			public void Visit(BeginProperty instruction)
			{
				var target = _properties.SingleOrDefault(_ => _.Name.EqualsIgnoreCase(instruction.Key.Resolve(_services).ToString()));
				if (target == null || instruction.Field == null)
					return;

				var fieldName = instruction.Field.Resolve(_services).ToString();
				var field = Table.Fields[fieldName];
				if (field == null)
					return;

				var fieldMetadata = _metadata.MetadataFor(field);
				Properties.Add(new MappedProperty(target, field, fieldMetadata));
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
		}
	}

	public class ModelQueryResult
	{
		public ModelQueryResult(ISchemaTableBase baseTable, IEnumerable<MappedProperty> properties)
		{
			BaseTable = baseTable;
			Properties = properties;
		}

		public ISchemaTableBase BaseTable { get; private set; }
		public IEnumerable<MappedProperty> Properties { get; private set; }
	}

	public class MappedProperty
	{
		public MappedProperty(PropertyInfo property, ISchemaField field, FieldSchemaMetadata metadata)
		{
			Property = property;
			Field = field;
			Metadata = metadata;
		}

		public PropertyInfo Property { get; private set; }
		public ISchemaField Field { get; private set; }
		public FieldSchemaMetadata Metadata { get; private set; }
	}
}

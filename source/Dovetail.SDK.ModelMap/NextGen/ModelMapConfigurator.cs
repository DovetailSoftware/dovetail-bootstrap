using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FChoice.Foundation.Schema;
using FubuCore;
using FubuCore.Reflection;
using StructureMap;

namespace Dovetail.SDK.ModelMap.NextGen
{
	public class ModelMapConfigurator<IN, OUT>
	{
		private readonly IContainer _container;
		private readonly ISchemaCache _schemaCache;
		public ModelMapConfig<IN, OUT> MapConfig { get; set; }

		public ModelMapConfigurator(IContainer container, ISchemaCache schemaCache)
		{
			_container = container;
			_schemaCache = schemaCache;
			MapConfig = new ModelMapConfig<IN, OUT>();
		}

		public T Get<T>()
		{
			return _container.GetInstance<T>();
		}

		public FieldConfig<IN> Field(string fieldName)
		{
			var fieldMap = new FieldConfig<IN>(fieldName);

			MapConfig.Fields.Add(fieldMap);

			return fieldMap;
		}

		public FieldConfig<IN> SelectField(string fieldName, Expression<Func<OUT, object>> expression)
		{
			var propertyInfo = ReflectionHelper.GetProperty(expression);

			var fieldMap = new FieldConfig<IN>(fieldName) {OutProperty = propertyInfo};

			MapConfig.Fields.Add(fieldMap);

			return fieldMap;
		}

		public void Join(string relationName, Action<ModelMapConfigurator<IN, OUT>> config)
		{
			if (!_schemaCache.IsValidRelation(MapConfig.BaseTable.Name, relationName))
			{
				throw new DovetailMappingException(2002, "Could not Join the relation {0} for the parent ModelMap<{1}, {2}> based on {3}.".ToFormat(relationName, typeof (IN).Name, typeof (OUT).Name, MapConfig.BaseTable.Name));
			}

			var joinConfigurator = new ModelMapConfigurator<IN, OUT>(_container, _schemaCache);

			config(joinConfigurator);

			var joinMap = joinConfigurator.MapConfig;

			joinMap.Parent = MapConfig;
			joinMap.ViaRelation = _schemaCache.GetRelation(MapConfig.BaseTable.Name, relationName);
			joinMap.BaseTable = joinMap.ViaRelation.SourceTable;

			MapConfig.Joins.Add(joinMap);
		}
	}

	public class Matches : FieldConfigOperator { }
	public class FieldConfigOperator { }

	public class FieldConfig
	{
		public string FieldName { get; set; }
		public PropertyInfo OutProperty { get; set; }
		public PropertyInfo InputProperty { get; set; }
		public object InputValue { get; set; }
		public FieldConfigOperator Operator { get; set; }
	}

	public class FieldConfig<IN> : FieldConfig
	{
		public FieldConfig(string fieldName)
		{
			FieldName = fieldName;
		}

		public void EqualTo(Expression<Func<IN, object>> expression)
		{
			var propertyInfo = ReflectionHelper.GetProperty(expression);

			Operator = new Matches();

			InputProperty = propertyInfo;
		}

		public void EqualTo(object value)
		{
			Operator = new Matches();

			InputValue = value;
		}
	}

	public class ModelMapConfig<IN, OUT>
	{
		public ModelMapConfig<IN, OUT> Parent { get; set; }
		public ISchemaTableBase BaseTable { get; set; }
		public ISchemaRelation ViaRelation { get; set; }

		public List<FieldConfig<IN>> Fields { get; private set; }
		public List<ModelMapConfig<IN, OUT>> Joins { get; private set; }

		public ModelMapConfig()
		{
			Fields = new List<FieldConfig<IN>>();
			Joins = new List<ModelMapConfig<IN, OUT>>();
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FChoice.Foundation.Schema;
using FubuCore;
using FubuCore.Reflection;
using StructureMap;

namespace Dovetail.SDK.ModelMap.NextGen
{
	public class ModelMapConfigurator<FILTER, OUT>
	{
		private readonly IContainer _container;
		private readonly ISchemaCache _schemaCache;
		public JoinConfig<FILTER, OUT> MapConfig { get; set; }

		public ModelMapConfigurator(IContainer container, ISchemaCache schemaCache)
		{
			_container = container;
			_schemaCache = schemaCache;
			var rootModelMapConfig = new ModelMapConfig<FILTER, OUT>();
			MapConfig = rootModelMapConfig;
			MapConfig.Root = rootModelMapConfig;
		}

		public ModelMapConfigurator(IContainer container, ISchemaCache schemaCache, ModelMapConfig<FILTER, OUT> root, JoinConfig<FILTER, OUT> parent)
		{
			_container = container;
			_schemaCache = schemaCache;
			MapConfig = new JoinConfig<FILTER, OUT>(root, parent);
		}

		public T Get<T>()
		{
			return _container.GetInstance<T>();
		}

		public FilterConfig<FILTER> Field(string fieldName)
		{
			var schemaField = getSchemaField(fieldName);	

			var filterConfig = new FilterConfig<FILTER>(schemaField);

			MapConfig.FilterConfigList.Add(filterConfig);

			return filterConfig;
		}

		public FilterConfig<FILTER> SelectField(string fieldName, Expression<Func<OUT, object>> expression)
		{
			var filterConfig = Field(fieldName);
			
			var propertyInfo = ReflectionHelper.GetProperty(expression);

			var selectConfig = new SelectConfig(filterConfig.SchemaField) {OutProperty = propertyInfo};
			MapConfig.SelectConfigList.Add(selectConfig);

			return filterConfig;
		}

		private ISchemaField getSchemaField(string fieldName)
		{
			if (!_schemaCache.IsValidField(MapConfig.BaseTable.Name, fieldName))
			{
				throw new DovetailMappingException(2003, "Could not find the field {0} for base object {1} in the schema.", fieldName, MapConfig.BaseTable.Name);
			}

			return _schemaCache.GetField(MapConfig.BaseTable.Name, fieldName);
		}

		public void Join(string relationName, Action<ModelMapConfigurator<FILTER, OUT>> config)
		{
			if (!_schemaCache.IsValidRelation(MapConfig.BaseTable.Name, relationName))
			{
				throw new DovetailMappingException(2002, "Could not Join the relation {0} for the parent ModelMap<{1}, {2}> based on {3}.".ToFormat(relationName, typeof (FILTER).Name, typeof (OUT).Name, MapConfig.BaseTable.Name));
			}

			var joinConfigurator = new ModelMapConfigurator<FILTER, OUT>(_container, _schemaCache, MapConfig.Root, MapConfig);
			var joinMap = joinConfigurator.MapConfig;
			joinMap.ViaRelation = _schemaCache.GetRelation(MapConfig.BaseTable.Name, relationName);
			joinMap.BaseTable = joinMap.ViaRelation.TargetTable;

			config(joinConfigurator);

			//tell parent map about child join map
			MapConfig.JoinConfigList.Add(joinMap);

			//tell root map about all of the configured filters so that they can be setable in the future.
			var editableFilters = joinMap.FilterConfigList.Where(f => f.IsEditable);
			MapConfig.Root.AddEditableFilters(editableFilters);
		}
	}

	public class FilterConfig
	{
		public ISchemaField SchemaField { get; set; }
		public PropertyInfo FilterProperty { get; set; }
		public object FilterValue { get; set; }
		public FilterOperator Operator { get; set; }

		public bool IsConfigured
		{
			get { return Operator != null && (FilterValue != null || IsEditable); }
		}

		public bool IsEditable
		{
			get { return FilterProperty != null; }
		}
	}

	public class SelectConfig
	{
		public ISchemaField SchemaField { get; private set; }
		public PropertyInfo OutProperty { get; set; }

		public SelectConfig(ISchemaField schemaField)
		{
			SchemaField = schemaField;
		}
	}

	public class FilterConfig<FILTER> : FilterConfig
	{
		public FilterConfig(ISchemaField schemaField)
		{
			SchemaField = schemaField;
		}

		public void EqualTo(object value)
		{
			Operator = new EqualsFilterOperator();

			FilterValue = value;
		}

		public void EqualTo(Expression<Func<FILTER, object>> expression)
		{
			FilterableBy(expression);

			Operator = new EqualsFilterOperator();
		}

		//TODO use interface to hide this method from the modelmap config's Override method?
		public FilterConfig<FILTER> FilterableBy(Expression<Func<FILTER, object>> expression)
		{
			var propertyInfo = ReflectionHelper.GetProperty(expression);

			FilterProperty = propertyInfo;

			return this;
		}
	}

	public interface IModelMapConfig<FILTER, OUT>
	{
		IEnumerable<SelectConfig> Selects { get; }
		IEnumerable<JoinConfig<FILTER, OUT>> Joins { get; }
		IEnumerable<FilterConfig<FILTER>> Filters { get; }
		
		ISchemaTableBase BaseTable { get; set; }
		FilterConfig<FILTER> SetFilter(Expression<Func<FILTER, object>> expression);
	}

	public class ModelMapConfig<FILTER, OUT> : JoinConfig<FILTER, OUT>, IModelMapConfig<FILTER, OUT>
	{
		private readonly IDictionary<PropertyInfo, FilterConfig<FILTER>> _editableFilters;

		public IEnumerable<SelectConfig> Selects { get { return SelectConfigList.ToArray(); } }
		public IEnumerable<JoinConfig<FILTER, OUT>> Joins { get { return JoinConfigList.ToArray(); } }
		public IEnumerable<FilterConfig<FILTER>> Filters { get { return FilterConfigList.ToArray(); } }

		public ModelMapConfig() : base(null, null)
		{
			_editableFilters = new Dictionary<PropertyInfo, FilterConfig<FILTER>>();
		}

		public FilterConfig<FILTER> SetFilter(Expression<Func<FILTER, object>> expression)
		{
			var propertyInfo = ReflectionHelper.GetProperty(expression);

			if (!_editableFilters.ContainsKey(propertyInfo))
			{
				throw new DovetailMappingException(2005, "Cannot set a non filterable property named {0}".ToFormat(propertyInfo.Name));
			}

			return _editableFilters[propertyInfo];
		}

		public void AddEditableFilters(IEnumerable<FilterConfig<FILTER>> filters)
		{
			filters.Each(filter => _editableFilters.Add(filter.FilterProperty, filter));
		}
	}

	public class JoinConfig<FILTER, OUT>
	{
		public ModelMapConfig<FILTER, OUT> Root { get; set; }
		public JoinConfig<FILTER, OUT> Parent { get; set; }
		public ISchemaTableBase BaseTable { get; set; }
		public ISchemaRelation ViaRelation { get; set; }

		public List<SelectConfig> SelectConfigList { get; private set; }
		public List<FilterConfig<FILTER>> FilterConfigList { get; private set; }
		public List<JoinConfig<FILTER, OUT>> JoinConfigList { get; private set; }
		
		public JoinConfig(ModelMapConfig<FILTER, OUT> root, JoinConfig<FILTER, OUT> parent)
		{
			Root = root;
			Parent = parent;
			SelectConfigList = new List<SelectConfig>();
			JoinConfigList = new List<JoinConfig<FILTER, OUT>>();
			FilterConfigList = new List<FilterConfig<FILTER>>();
		}
	}
}
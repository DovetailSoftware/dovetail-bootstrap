using System;
using System.Linq;
using FChoice.Foundation.Schema;
using FubuCore;
using StructureMap;

namespace Dovetail.SDK.ModelMap.NextGen
{
	public interface IModelMapConfigFactory<FILTER, OUT>
	{
		RootModelMapConfig<FILTER, OUT> Create(string objectName, Action<ModelMapConfigurator<FILTER, OUT>> config);
	}

	public class ModelMapConfigFactory<FILTER, OUT> : IModelMapConfigFactory<FILTER, OUT>
	{
		private readonly IContainer _container;
		private readonly ISchemaCache _schemaCache;

		public ModelMapConfigFactory(IContainer container, ISchemaCache schemaCache)
		{
			_container = container;
			_schemaCache = schemaCache;
		}

		public RootModelMapConfig<FILTER, OUT> Create(string objectName, Action<ModelMapConfigurator<FILTER, OUT>> config)
		{
			if (!_schemaCache.IsValidTableOrView(objectName))
			{
				throw new DovetailMappingException(2001, "Could not create a ModelMap<{0}, {1}> for the schema object named {2}.".ToFormat(typeof (FILTER).Name, typeof (OUT).Name, objectName));
			}

			var configurator = new ModelMapConfigurator<FILTER, OUT>(_container, _schemaCache);
			
			var map = configurator.MapConfig;
			ISchemaTableBase baseTable = _schemaCache.IsValidTable(objectName) ? _schemaCache.Tables[objectName] as ISchemaTableBase : _schemaCache.Views[objectName] as ISchemaTableBase;
			map.BaseTable = baseTable;

			config(configurator);

			var editableFilters = map.Filters.Where(f => f.IsEditable);
			map.Root.AddEditableFilters(editableFilters);

			if(!map.Selects.Any())
			{
				throw new DovetailMappingException(2006, "No fields were selected by the '{0}' map", objectName);
			}

			return map as RootModelMapConfig<FILTER, OUT>;
		}
	}
}
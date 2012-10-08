using System;
using FChoice.Foundation.Schema;
using FubuCore;
using StructureMap;

namespace Dovetail.SDK.ModelMap.NextGen
{
	public class ModelMapFactory<FILTER, OUT>
	{
		private readonly IContainer _container;
		private readonly ISchemaCache _schemaCache;

		public ModelMapFactory(IContainer container, ISchemaCache schemaCache)
		{
			_container = container;
			_schemaCache = schemaCache;
		}

		//TODO see if the resulting modelmap can be cached for reuse.

		public RootModelMapConfig<FILTER, OUT> Create(string objectName, Action<ModelMapConfigurator<FILTER, OUT>> config)
		{
			if (!_schemaCache.IsValidTableOrView(objectName))
			{
				throw new DovetailMappingException(2001, "Could not create a ModelMap<{0}, {1}> for the schema object named {2}.".ToFormat(typeof (FILTER).Name, typeof (OUT).Name, objectName));
			}

			var configurator = new ModelMapConfigurator<FILTER, OUT>(_container, _schemaCache);
			
			var map = configurator.MapConfig;
			var baseTable = _schemaCache.IsValidTable(objectName) ? _schemaCache.Tables[objectName] as ISchemaTableBase : _schemaCache.Views[objectName] as ISchemaTableBase;
			map.BaseTable = baseTable;

			config(configurator);

			return map as RootModelMapConfig<FILTER, OUT>;
		}
	}
}
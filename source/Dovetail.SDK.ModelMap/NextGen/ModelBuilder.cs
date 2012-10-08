using System.Collections.Generic;
using FChoice.Common.Data;

namespace Dovetail.SDK.ModelMap.NextGen
{
	public class MapQuerySqlGenerator
	{
		public SqlHelper Generate(MapQueryConfig query)
		{
			var sql = BuildSql();

			var sqlHelper = new SqlHelper(sql);

			AddParameters(sqlHelper);

			return sqlHelper;
		}

		private void AddParameters(SqlHelper sqlHelper)
		{
			
		}

		private string BuildSql()
		{
			return "";
		}
	}


	public class ModelBuilder<FILTER, OUT>
	{
		private readonly MapQueryConfigFactory<FILTER, OUT> _mapQueryConfigFactory;

		public ModelBuilder(MapQueryConfigFactory<FILTER, OUT> mapQueryConfigFactory)
		{
			_mapQueryConfigFactory = mapQueryConfigFactory;
		}

		public IEnumerable<OUT> Execute(FILTER filterModel)
		{
			var query = _mapQueryConfigFactory.Create(filterModel);

			return new OUT[0];
		}
	}
}
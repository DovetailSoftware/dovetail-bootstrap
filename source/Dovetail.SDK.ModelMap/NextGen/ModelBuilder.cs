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


	public class ModelBuilder<IN, OUT>
	{
		private readonly MapQueryConfigFactory<IN, OUT> _mapQueryConfigFactory;

		public ModelBuilder(MapQueryConfigFactory<IN, OUT> mapQueryConfigFactory)
		{
			_mapQueryConfigFactory = mapQueryConfigFactory;
		}

		public IEnumerable<OUT> Execute(IN inputModel)
		{
			var query = _mapQueryConfigFactory.Create(inputModel);



			return new OUT[0];
		}
	}
}
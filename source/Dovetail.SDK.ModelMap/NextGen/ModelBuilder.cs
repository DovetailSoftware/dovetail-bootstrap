using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap;
using FChoice.Common.Data;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NextGen
{
	public interface IModelBuilder<FILTER, OUT> where OUT : new()
	{
		IEnumerable<OUT> Execute(FILTER filterModel);
		IEnumerable<OUT> Execute(FILTER filterModel, Action<IModelMapConfig<FILTER,OUT>> action);
	}

	public interface IModelMap<FILTER, OUT>
	{
		IModelMapConfig<FILTER, OUT> Create(IModelMapConfigFactory<FILTER, OUT> mapFactory);
	}

	public class ModelBuilder<FILTER, OUT> : IModelBuilder<FILTER, OUT> where OUT : new()
	{
		private readonly IMapQueryFactory<FILTER, OUT> _mapQueryFactory;
		private readonly IModelMapConfigFactory<FILTER, OUT> _mapFactory;
		private readonly IModelMap<FILTER, OUT> _map;
		private readonly ILogger _logger;

		public ModelBuilder(IMapQueryFactory<FILTER, OUT> mapQueryFactory, IModelMapConfigFactory<FILTER, OUT> mapFactory, IModelMap<FILTER, OUT> map, ILogger logger)
		{
			_mapQueryFactory = mapQueryFactory;
			_mapFactory = mapFactory;
			_map = map;
			_logger = logger;
		}

		public IEnumerable<OUT> Execute(FILTER filterModel)
		{
			return Execute(filterModel, config => { });
		}

		public IEnumerable<OUT> Execute(FILTER filterModel, Action<IModelMapConfig<FILTER, OUT>> action)
		{
			var modelMap = _map.Create(_mapFactory);

			action(modelMap);

			var query = _mapQueryFactory.Create(filterModel, modelMap);

			var sqlHelper = BuildSql(query);

			//Console.WriteLine(sqlHelper.CommandText);
			_logger.LogDebug("SQL Generated:", sqlHelper.CommandText);

			var results = new List<OUT>();
			using (var reader = sqlHelper.ExecuteReader())
			{
				while (reader.Read())
				{
					var result = new OUT();

					foreach (var s in query.Selects)
					{
						var value = reader[s.Index];
						s.OutProperty.SetValue(result, value, null);
					}

					results.Add(result);
				}
			}

			return results;
		}

		public static SqlHelper BuildSql(MapQueryConfig query)
		{
			var sqlHelper = new SqlHelper();

			var selectClause = query.Selects.OrderBy(s=>s.Index).Select(s => "{0}.{1}".ToFormat(s.Alias, s.Field.Name)).Join(",");

			var joinClause = query.Joins.Select(j => j.JoinSql).Join(" ");

			var whereClause = "";
			if(query.Wheres.Any())
			{
				whereClause = "WHERE " + query.Wheres.Select(w => w.Operator.Render(w, sqlHelper)).Join(" AND ");
			}

			var sql = "SELECT {0} {1} {2}".ToFormat(selectClause, joinClause, whereClause);

			sqlHelper.CommandText = sql;

			return sqlHelper;
		}
	}
}
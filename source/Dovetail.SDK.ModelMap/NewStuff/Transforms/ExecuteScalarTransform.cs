using System;
using Dovetail.SDK.Bootstrap;
using FChoice.Common.Data;

namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public class ExecuteScalarTransform : IMappingTransform
	{
		public object Execute(TransformContext context)
		{
			var logger = context.Service<ILogger>();
			logger.LogDebug("ExecuteScalar transform");

			var sql = context.Arguments.Get<string>("sql");
			var dataType = context.Arguments.Get<string>("dataType");

			var sqlHelper = new SqlHelper(sql);
			foreach (var pair in context.Arguments)
			{
				logger.LogDebug("arg {0}:{1}", pair.Key, pair.Value);

				if (!pair.Key.ToLower().StartsWith("param.")) continue;

				var key = pair.Key.Substring(6);
				sqlHelper.Parameters.Add(key, pair.Value);
			}

			var result = sqlHelper.ExecuteScalar();

			var targetType = PropertyTypes.Parse(dataType);
			if (result == null || DBNull.Value.Equals(result))
				return null;

			return Convert.ChangeType(result, targetType);
		}
	}
}
using System;
using FChoice.Common.Data;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public class SqlCountTransform : IMappingTransform
	{
		public object Execute(TransformContext context)
		{
			var table = context.Arguments.Get<string>("table");
			var relation = context.Arguments.Get<string>("relation");
			var objid = context.Arguments.Get<int>("objid");

			var sqlHelper = new SqlHelper("SELECT COUNT(1) FROM table_{0} where {1} = {{0}}".ToFormat(table, relation));
			sqlHelper.Parameters.Add(relation, objid);

			return Convert.ToInt32(sqlHelper.ExecuteScalar());
		}
	}
}
using System.Globalization;
using FChoice.Common.Data;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NextGen
{
	public abstract class FilterOperator
	{
		public abstract string Render(WhereItem config, SqlHelper db);
	}

	public abstract class OperatorFilterOperator : FilterOperator
	{
		public override string Render(WhereItem config, SqlHelper db)
		{
			int paramNum = db.Parameters.Count;
			var paramName = "param" + paramNum.ToString(CultureInfo.InvariantCulture);

			var databaseParameterFactory = new DatabaseParameterFactory(DbProviderFactory.Provider);

			var parameter = databaseParameterFactory.Create(paramName, config.Field, config.Value);
			db.Parameters.Add(paramName, parameter);

			return "{0}.{1} {2} {{{3}}}".ToFormat(config.Alias, config.Field.Name, Operator, paramNum);
		}

		protected abstract string Operator { get; }
	}

	public class EqualsFilterOperator : OperatorFilterOperator
	{
		protected override string Operator
		{
			get { return "="; }
		}
	}

	public class NotEqualsFilterOperator : OperatorFilterOperator
	{
		protected override string Operator
		{
			get { return "<>"; }
		}
	}

	public class LessThanFilterOperator : OperatorFilterOperator
	{
		protected override string Operator
		{
			get { return "<"; }
		}
	}

	public class GreaterThanFilterOperator : OperatorFilterOperator
	{
		protected override string Operator
		{
			get { return ">"; }
		}
	}
}
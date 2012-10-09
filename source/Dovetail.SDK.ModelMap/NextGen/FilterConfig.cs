using System;
using System.Globalization;
using FChoice.Common.Data;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Clarify.Schema;
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
			var isOracle = DbProviderFactory.IsOracle();

			int paramNum = db.Parameters.Count;
			var paramName = "param" + paramNum.ToString(CultureInfo.InvariantCulture);

			var databaseParameterFactory = new DatabaseParameterFactory(DbProviderFactory.Provider);

			var fieldName = config.Field.Name;
			var value = config.Value;

			if(isOracle && config.Field.IsSearchable && config.Field.CommonType == (int) SchemaCommonType.String)
			{
				fieldName = "S_" + fieldName;
				value = Convert.ToString(value).ToUpperInvariant();
			}

			var parameter = databaseParameterFactory.Create(paramName, config.Field, value);
			db.Parameters.Add(paramName, parameter);
			
			return "{0}.{1} {2} {{{3}}}".ToFormat(config.Alias, fieldName, Operator, paramNum);
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
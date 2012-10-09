using System.Data;
using FChoice.Common;
using FChoice.Common.Data;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Clarify.Schema;
using FChoice.Foundation.Schema;

namespace Dovetail.SDK.ModelMap.NextGen
{
	public class DatabaseParameterFactory
	{
		private readonly ISchemaCache _schemaCache;
		private readonly DbProvider _dbProvider;

		public DatabaseParameterFactory(DbProvider dbProvider)
		{
			_schemaCache = ClarifyApplication.Instance.SchemaCache;
			_dbProvider = dbProvider;
		}

		public DatabaseParameterFactory(ISchemaCache schemaCache, DbProvider dbProvider)
		{
			_schemaCache = schemaCache;
			_dbProvider = dbProvider;
		}

		public IDbDataParameter Create(string paramName, ISchemaField schemaField, object paramValue)
		{
			IDbDataParameter databaseParameter = _dbProvider.CreateParameter(paramName, paramValue);

			if (ShouldStringParameterTypeBeSet())
			{
				ConfigureStringDatabaseParameter(databaseParameter, _schemaCache, _dbProvider, schemaField);
			}

			return databaseParameter;
		}

		public IDbDataParameter Create(string paramName, string tableName, string fieldName, object paramValue)
		{
			var schemaField = _schemaCache.GetField(tableName, fieldName);

			return Create(paramName, schemaField, paramValue);
		}


		public static void ConfigureStringDatabaseParameter(IDbDataParameter databaseParameter, ISchemaCache schemaCache, DbProvider dbProvider, ISchemaField schemaField)
		{
			databaseParameter.Direction = ParameterDirection.Input;

			if (schemaField == null || ((int)schemaField.DataType != (int)SchemaCommonType.String))
				return;

			bool isParameterUnicode = (schemaField.IsUnicode || schemaCache.IsDatabaseUnicode);

			if (schemaField.DatabaseType == (int)SchemaDataType.ClobOrText)
			{
				dbProvider.SetDatabaseParameterTypeForClobText(databaseParameter, isParameterUnicode);
			}
			else if (dbProvider.ProviderName != DbProviderFactory.ORACLE_PROVIDER_NAME)
			{
				if (schemaField.Length > 0)
				{
					databaseParameter.Size = schemaField.Length;
				}
				databaseParameter.DbType = isParameterUnicode ? DbType.String : DbType.AnsiString;
			}
		}

		public const string DisableSettingStringParameterType = "fchoice.disable.setstringparameterdbtype";

		public virtual bool ShouldStringParameterTypeBeSet()
		{
			string setStringParameterTypeValue = FCConfiguration.Current[DisableSettingStringParameterType];
			bool isConfigurationSettingPresent = StringUtil.IsNonEmptyString(setStringParameterTypeValue);

			if (isConfigurationSettingPresent && setStringParameterTypeValue.ToUpperInvariant() == "TRUE")
			{
				return false;
			}

			return true;
		}
	}
}
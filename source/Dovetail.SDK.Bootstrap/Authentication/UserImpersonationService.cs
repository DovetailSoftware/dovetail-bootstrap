using System;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Common.Data;
using FChoice.Foundation;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Authentication
{
	public interface IUserImpersonationService
	{
		/// <summary>
		/// Stop impersonation by the given login
		/// </summary>
		/// <returns>Act entry objid of the cancelled action</returns>
		int StopImpersonating(string impersonatingUserLogin);
		/// <summary>
		/// Start impersonation by the given login for the user login to be impersonated.
		/// </summary>
		/// <returns>Act entry objid of the impersonation creation action</returns>
		int StartImpersonation(string impersonatingUserLogin, string userLoginBeingImpersonated);
		string GetImpersonatedLoginFor(string login);
	}

	public class UserImpersonationService : IUserImpersonationService
	{
		private readonly IApplicationClarifySession _applicationSession;
		private readonly IListCache _listCache;
		private readonly DovetailDatabaseSettings _settings;
		private readonly ILogger _logger;

		public UserImpersonationService(IApplicationClarifySession applicationSession,
			IListCache listCache,
			DovetailDatabaseSettings settings,
			ILogger logger)
		{
			_applicationSession = applicationSession;
			_listCache = listCache;
			_settings = settings;
			_logger = logger;
		}

		public string GetImpersonatedLoginFor(string login)
		{
			if (!_settings.IsImpersonationEnabled)
			{
				return null;
			}

			var sql = new SqlHelper("SELECT p.login_name, p.status FROM table_user u, table_user p WHERE u.login_name = {0} AND p.objid = u.user2proxy_user");
			sql.Parameters.Add("login", login);
			var result = sql.ExecuteReader();

			while (result.Read())
			{
				var status = Convert.ToInt32(result["status"]);
				var impersonatedLoginFor = result["login_name"].ToString();

				if (status == 1)
				{
					return impersonatedLoginFor;
				}
				else
				{
					_logger.LogDebug("Cancelling the impersonation of INACTIVE user {0} by user {1}.".ToFormat(impersonatedLoginFor, login));
					cancelImpersonation(login, impersonatedLoginFor);
					return null;
				}
			}

			return null;
		}

		public int StopImpersonating(string impersonatingUserLogin)
		{
			int result = -1;
			if (!_settings.IsImpersonationEnabled || impersonatingUserLogin.IsEmpty())
			{
				return result; //nothing to do
			}

			var impersonatedUsername = GetImpersonatedLoginFor(impersonatingUserLogin);

			if (GetImpersonatedLoginFor(impersonatingUserLogin).IsNotEmpty())
			{
				result = cancelImpersonation(impersonatingUserLogin, impersonatedUsername);
			};

			return result;
		}

		private int cancelImpersonation(string impersonatingUserLogin, string impersonatedUsername)
		{
			_logger.LogDebug("Cancelling the impersonation of user {0} by user {1}.".ToFormat(impersonatedUsername,
				impersonatingUserLogin));

			//create activity entry for impersonatedUserLogin completion

			var result = CreateActEntry(impersonatingUserLogin, impersonatedUsername, 94003, "Revert impersonation of " + impersonatedUsername);

			cancelImpersonationFor(impersonatingUserLogin);

			return result;
		}

		public int StartImpersonation(string impersonatingUserLogin, string userLoginBeingImpersonated)
		{
			int result = -1;
			if (!_settings.IsImpersonationEnabled)
			{
				return result; //nothing to do
			}

			//create activity entry for impersonatedUserLogin creation
			var dataset = _applicationSession.CreateDataSet();

			var proxyUserGeneric = dataset.CreateGenericWithFields("user", "objid");
			proxyUserGeneric.Filter(f => f.Equals("login_name", impersonatingUserLogin));

			var proxiedUserGeneric = dataset.CreateGenericWithFields("user", "objid");
			proxiedUserGeneric.Filter(f => f.Equals("login_name", userLoginBeingImpersonated));
			var proxiedEmployeeGeneric = proxiedUserGeneric.TraverseWithFields("user2employee", "allow_proxy");

			dataset.Query(proxyUserGeneric, proxiedUserGeneric);

			if (proxyUserGeneric.Count < 1)
			{
				throw new ArgumentException("The impersonating user {0} does not exist.".ToFormat(impersonatingUserLogin), "impersonatingUserLogin");
			}

			if (proxiedUserGeneric.Count < 1)
			{
				throw new ArgumentException("The user being impersonated {0} does not exist.".ToFormat(userLoginBeingImpersonated), "userLoginBeingImpersonated");
			}

			if (proxiedEmployeeGeneric.Rows[0].AsInt("allow_proxy") != 1)
			{
				throw new ArgumentException("The user being impersonated {0} does not allow others to impersonate them. The employee record for this user must have allow_proxy set to 1.".ToFormat(userLoginBeingImpersonated), "userLoginBeingImpersonated");
			}

			cancelImpersonationFor(impersonatingUserLogin);

			_logger.LogDebug("Setting up user {0} as an impersonator of user {1}.".ToFormat(userLoginBeingImpersonated, impersonatingUserLogin));

			//create act entry for impersonatedUserLogin creation
			result = CreateActEntry(impersonatingUserLogin, userLoginBeingImpersonated, 94002, "Impersonate " + userLoginBeingImpersonated);

			createImpersonationFor(impersonatingUserLogin, userLoginBeingImpersonated);
			return result;
		}

		private int CreateActEntry(string impersonatingUserLogin, string impersonatedUserLogin, int actCode, string message)
		{
			var dataset = _applicationSession.CreateDataSet();

			var userGeneric = dataset.CreateGenericWithFields("user", "objid");
			userGeneric.Filter(f => f.Equals("login_name", impersonatingUserLogin));
			userGeneric.Query();

			if (userGeneric.Count < 1)
			{
				throw new Exception("User {0} does not exist.".ToFormat(impersonatingUserLogin));
			}

			var actEntryGeneric = dataset.CreateGeneric("act_entry");
			var actEntry = actEntryGeneric.AddNew();
			actEntry["act_code"] = actCode;
			actEntry["entry_time"] = FCGeneric.NOW_DATE;
			actEntry["addnl_info"] = message;
			actEntry["proxy"] = impersonatedUserLogin;
			actEntry.RelateByID(userGeneric.Rows[0].DatabaseIdentifier(), "act_entry2user");
			actEntry.RelateByID(_listCache.GetGbstElmRankObjID("Activity Name", actCode), "entry_name2gbst_elm");
			actEntry.Update();

			return actEntry.DatabaseIdentifier();
		}

		public static void cancelImpersonationFor(string impersonatingUserLogin)
		{
			var sql = new SqlHelper("update table_user set user2proxy_user = NULL where login_name = {0}");
			sql.Parameters.Add("login", impersonatingUserLogin);
			sql.ExecuteNonQuery();
		}

		public static void createImpersonationFor(string impersonatingUserLogin, string userLoginBeingImpersonated)
		{
			var sql = new SqlHelper("update table_user set user2proxy_user = (select objid from table_user where login_name = {0}) where login_name = {1}");
			sql.Parameters.Add("login", userLoginBeingImpersonated);
			sql.Parameters.Add("proxylogin", impersonatingUserLogin);
			sql.ExecuteNonQuery();
		}
	}
}

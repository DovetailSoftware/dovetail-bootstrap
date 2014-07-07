using System;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Common.Data;
using FChoice.Foundation;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Authentication
{
	public interface IUserProxyService
	{
		void CancelProxy(string proxyUserLogin);
		void CreateProxy(string proxyUserLogin, string userBeingProxiedLogin);
		string GetCurrentProxiedLoginFor(string login);
	}

	public class UserProxyService : IUserProxyService
	{
		private readonly IClarifySessionCache _sessionCache;
		private readonly IListCache _listCache;
		private readonly ILogger _logger;

		public UserProxyService(IClarifySessionCache sessionCache, 
			IListCache listCache, 
			ILogger logger)
		{
			_sessionCache = sessionCache;
			_listCache = listCache;
			_logger = logger;
		}

		public string GetCurrentProxiedLoginFor(string login)
		{
			var sql = new SqlHelper("SELECT p.login_name FROM table_user u, table_user p WHERE u.login_name = {0} AND p.objid = u.user2proxy_user");
			sql.Parameters.Add("login", login);
			var result = sql.ExecuteScalar();

			if (result == null || result == DBNull.Value) return null;

			return Convert.ToString(result);
		}

		public void CancelProxy(string proxyUserLogin)
		{
			if (proxyUserLogin.IsEmpty())
			{
				return; //nothing to do
			}

			var session = _sessionCache.GetSession(proxyUserLogin);
			var proxiedUserName = session.ProxyUserName;

			if (proxiedUserName != null && HasProxyFor(proxyUserLogin))
			{
				_logger.LogDebug("Cancelling the proxy of user {0} by user {1}.".ToFormat(session.UserName, session.ProxyUserName));

				//create activity entry for proxy completion
				CreateActEntry("Revert impersonation of " + proxiedUserName, proxiedUserName, 94003, session.ProxyUserId);

				CancelProxyFor(proxiedUserName);

				//eject session as it is proxied and should not be used going forward
				_logger.LogDebug("Ejecting existing session for user {0}. New sessions will not be proxied.".ToFormat(proxyUserLogin));
				_sessionCache.EjectSession(proxiedUserName);
				return;
			}

			_sessionCache.EjectSession(proxyUserLogin);
		}

		public void CreateProxy(string proxyUserLogin, string userBeingProxiedLogin)
		{
			//create activity entry for proxy creation
			var applicationSession = _sessionCache.GetApplicationSession();
			var dataset = applicationSession.CreateDataSet();

			var proxyUserGeneric = dataset.CreateGenericWithFields("user", "objid");
			proxyUserGeneric.Filter(f => f.Equals("login_name", proxyUserLogin));

			var proxiedUserGeneric = dataset.CreateGenericWithFields("user", "objid");
			proxiedUserGeneric.Filter(f => f.Equals("login_name", userBeingProxiedLogin));

			dataset.Query(proxyUserGeneric, proxiedUserGeneric);

			if (proxyUserGeneric.Count < 1)
			{
				throw new ArgumentException("The proxy user {0} does not exist.".ToFormat(proxyUserLogin), "proxyUserLogin");
			}

			if (proxiedUserGeneric.Count < 1)
			{
				throw new ArgumentException("The user being proxied {0} does not exist.".ToFormat(userBeingProxiedLogin), "userBeingProxiedLogin");
			}

			CancelProxyFor(proxyUserLogin);

			_logger.LogDebug("Setting up user {0} as a proxy of user {1}.".ToFormat(userBeingProxiedLogin, proxyUserLogin));

			//create act entry for proxy creation
			CreateActEntry("Impersonate " + userBeingProxiedLogin, userBeingProxiedLogin, 94002, proxyUserGeneric.Rows[0].DatabaseIdentifier());

			CreateProxyFor(proxyUserLogin, userBeingProxiedLogin);

			_logger.LogDebug("Ejecting existing session for user {0}. New sessions will be proxied.".ToFormat(proxyUserLogin));
			_sessionCache.EjectSession(proxyUserLogin);
		}

		private void CreateActEntry(string message, string proxy, int actCode, int userId)
		{
			var applicationSession = _sessionCache.GetApplicationSession();
			var dataset = applicationSession.CreateDataSet();
			var actEntryGeneric = dataset.CreateGeneric("act_entry");
			var actEntry = actEntryGeneric.AddNew();
			actEntry["act_code"] = actCode;
			actEntry["entry_time"] = FCGeneric.NOW_DATE;
			actEntry["addnl_info"] = message;
			actEntry["proxy"] = proxy;
			actEntry.RelateByID(userId, "act_entry2user");
			actEntry.RelateByID(_listCache.GetGbstElmRankObjID("Activity Name", actCode), "entry_name2gbst_elm");
			actEntry.Update();
		}

		public bool HasProxyFor(string proxyUserLogin)
		{
			var sql = new SqlHelper("SELECT user2proxy_user FROM table_user WHERE login_name = {0}");
			sql.Parameters.Add("login", proxyUserLogin);
			var result = sql.ExecuteScalar();

			return (result != null && result != DBNull.Value);
		}
		public static void CancelProxyFor(string proxyUserLogin)
		{
			var sql = new SqlHelper("update table_user set user2proxy_user = NULL where login_name = {0}");
			sql.Parameters.Add("login", proxyUserLogin);
			sql.ExecuteNonQuery();
		}

		public static void CreateProxyFor(string proxyUserLogin, string userBeingProxiedLogin)
		{
			var sql = new SqlHelper("update table_user set user2proxy_user = (select objid from table_user where login_name = {0}) where login_name = {1}");
			sql.Parameters.Add("login", userBeingProxiedLogin);
			sql.Parameters.Add("proxylogin", proxyUserLogin);
			sql.ExecuteNonQuery();
		}
	}
}
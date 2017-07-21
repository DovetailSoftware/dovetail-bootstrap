using System;
using System.Collections.Generic;
using FChoice.Common.Data;
using FChoice.Foundation.Clarify;
using FChoice.Toolkits.Clarify.FieldOps;
using FChoice.Toolkits.Clarify.Interfaces;
using FChoice.Toolkits.Clarify.Support;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface IClarifySessionProxy
	{
		ClarifySession Session { get; }
	}

	public class ClarifySessionProxy : IClarifySession, IClarifySessionProxy
	{
		private readonly string _username;
		private readonly IClarifySessionCache _cache;

		public ClarifySessionProxy(string username, IClarifySessionCache cache)
		{
			_username = username;
			_cache = cache;
		}

		private IClarifySession session
		{
			get { return _cache.GetSession(_username); }
		}

		public Guid Id { get { return session.Id; } }
		public string UserName { get { return session.UserName; } }
		public int SessionEmployeeID { get { return session.SessionEmployeeID; } }
		public int SessionUserID { get { return session.SessionUserID; } }
		public string SessionEmployeeSiteID { get { return session.SessionEmployeeSiteID; } }
		public string[] Permissions { get { return session.Permissions; } }
		public IEnumerable<string> DataRestriction { get { return session.DataRestriction; } }

		public ClarifyDataSet CreateDataSet()
		{
			return session.CreateDataSet();
		}

		public SupportToolkit CreateSupportToolkit()
		{
			return session.CreateSupportToolkit();
		}

		public FieldOpsToolkit CreateFieldOpsToolkit()
		{
			return session.CreateFieldOpsToolkit();
		}

		public InterfacesToolkit CreateInterfacesToolkit()
		{
			return session.CreateInterfacesToolkit();
		}

		public SqlHelper CreateSqlHelper(string sql)
		{
			return session.CreateSqlHelper(sql);
		}

		public void RefreshContext()
		{
			session.RefreshContext();
		}

		public void Close()
		{
			session.Close();
		}

		public ClarifySession Session { get { return session.As<IClarifySessionProxy>().Session; } }
	}
}
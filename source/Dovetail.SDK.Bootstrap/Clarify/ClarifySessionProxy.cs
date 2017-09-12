using System;
using System.Collections.Generic;
using FChoice.Common.Data;
using FChoice.Foundation.Clarify;
using FChoice.Toolkits.Clarify.Contracts;
using FChoice.Toolkits.Clarify.DepotRepair;
using FChoice.Toolkits.Clarify.FieldOps;
using FChoice.Toolkits.Clarify.Interfaces;
using FChoice.Toolkits.Clarify.Logistics;
using FChoice.Toolkits.Clarify.Quality;
using FChoice.Toolkits.Clarify.Sales;
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
		private readonly ICurrentSDKUser _user;
		private readonly IClarifySessionCache _cache;

		public ClarifySessionProxy(ICurrentSDKUser user, IClarifySessionCache cache)
		{
			_user = user;
			_cache = cache;
		}

		private IClarifySession session
		{
			get { return _cache.GetSession(_user.Username); }
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

		public ContractsToolkit CreateContractsToolkit()
		{
			var toolkit = session.CreateContractsToolkit();
			if (_user.ImpersonatingUsername.IsNotEmpty())
				toolkit.ImpersonatorUsername = _user.ImpersonatingUsername;

			return toolkit;
		}

		public DepotRepairToolkit CreateDepotRepairToolkit()
		{
			var toolkit = session.CreateDepotRepairToolkit();
			if (_user.ImpersonatingUsername.IsNotEmpty())
				toolkit.ImpersonatorUsername = _user.ImpersonatingUsername;

			return toolkit;
		}

		public SalesToolkit CreateSalesToolkit()
		{
			var toolkit = session.CreateSalesToolkit();
			if (_user.ImpersonatingUsername.IsNotEmpty())
				toolkit.ImpersonatorUsername = _user.ImpersonatingUsername;

			return toolkit;
		}

		public SupportToolkit CreateSupportToolkit()
		{
			var toolkit = session.CreateSupportToolkit();
			if (_user.ImpersonatingUsername.IsNotEmpty())
				toolkit.ImpersonatorUsername = _user.ImpersonatingUsername;

			return toolkit;
		}

		public FieldOpsToolkit CreateFieldOpsToolkit()
		{
			var toolkit = session.CreateFieldOpsToolkit();
			if (_user.ImpersonatingUsername.IsNotEmpty())
				toolkit.ImpersonatorUsername = _user.ImpersonatingUsername;

			return toolkit;
		}

		public InterfacesToolkit CreateInterfacesToolkit()
		{
			var toolkit = session.CreateInterfacesToolkit();
			if (_user.ImpersonatingUsername.IsNotEmpty())
				toolkit.ImpersonatorUsername = _user.ImpersonatingUsername;

			return toolkit;
		}

		public LogisticsToolkit CreateLogisticsToolkit()
		{
			var toolkit = session.CreateLogisticsToolkit();
			if (_user.ImpersonatingUsername.IsNotEmpty())
				toolkit.ImpersonatorUsername = _user.ImpersonatingUsername;

			return toolkit;
		}

		public QualityToolkit CreateQualityToolkit()
		{
			var toolkit = session.CreateQualityToolkit();
			if (_user.ImpersonatingUsername.IsNotEmpty())
				toolkit.ImpersonatorUsername = _user.ImpersonatingUsername;

			return toolkit;
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
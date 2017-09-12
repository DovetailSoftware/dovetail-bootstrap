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

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface IApplicationClarifySession : IClarifySession
	{
	}

	public interface IClarifySession
	{
		Guid Id { get; }
		string UserName { get; }
		int SessionEmployeeID { get; }
		int SessionUserID { get; }
		string SessionEmployeeSiteID { get; }
		ClarifyDataSet CreateDataSet();

		ContractsToolkit CreateContractsToolkit();
		DepotRepairToolkit CreateDepotRepairToolkit();
		FieldOpsToolkit CreateFieldOpsToolkit();
		InterfacesToolkit CreateInterfacesToolkit();
		LogisticsToolkit CreateLogisticsToolkit();
		QualityToolkit CreateQualityToolkit();
		SalesToolkit CreateSalesToolkit();
		SupportToolkit CreateSupportToolkit();


		SqlHelper CreateSqlHelper(string sql);
		string[] Permissions { get; }
		IEnumerable<string> DataRestriction { get; }
		void RefreshContext();
		void Close();
	}

	public class ClarifySessionWrapper : IApplicationClarifySession, IClarifySessionProxy
	{
		public ClarifySessionWrapper(ClarifySession clarifySession)
		{
			ClarifySession = clarifySession;
		}

		public ClarifySession ClarifySession { get; set; }

		public string SessionEmployeeSiteID
		{
			get { return Convert.ToString(ClarifySession["employee.site.site_id"]); }
		}

		public string[] Permissions
		{
			get { return ClarifySession.GetAssignedPermissions(); }
		}

		public Guid Id
		{
			get { return ClarifySession.SessionID; }
		}

		public string UserName
		{
			get { return ClarifySession.UserName; }
		}

		public int SessionEmployeeID
		{
			get { return Convert.ToInt32(ClarifySession["employee.id"]); }
		}

		public int SessionUserID
		{
			get { return Convert.ToInt32(ClarifySession["user.id"]); }
		}

		public IEnumerable<string> DataRestriction
		{
			get
			{
				var restrictionGroup = ClarifySession.RestrictionGroup;

				return restrictionGroup == null ? new string[0] : restrictionGroup.Restrictions;
			}
		}

		public ClarifyDataSet CreateDataSet()
		{
			var clarifyDataSet = new ClarifyDataSet(ClarifySession);

			return clarifyDataSet;
		}

		public ContractsToolkit CreateContractsToolkit()
		{
			return new ContractsToolkit(ClarifySession);
		}

		public DepotRepairToolkit CreateDepotRepairToolkit()
		{
			return new DepotRepairToolkit(ClarifySession);
		}

		public SalesToolkit CreateSalesToolkit()
		{
			return new SalesToolkit(ClarifySession);
		}

		public SupportToolkit CreateSupportToolkit()
		{
			return new SupportToolkit(ClarifySession);
		}

		public FieldOpsToolkit CreateFieldOpsToolkit()
		{
			return new FieldOpsToolkit(ClarifySession);
		}

		public InterfacesToolkit CreateInterfacesToolkit()
		{
			return new InterfacesToolkit(ClarifySession);
		}

		public LogisticsToolkit CreateLogisticsToolkit()
		{
			return new LogisticsToolkit(ClarifySession);
		}

		public QualityToolkit CreateQualityToolkit()
		{
			return new QualityToolkit(ClarifySession);
		}

		public SqlHelper CreateSqlHelper(string sql)
		{
			return new SqlHelper(sql);
		}

		public void RefreshContext()
		{
			ClarifySession.RefreshContext();
		}

		public void Close()
		{
			ClarifySession.CloseSession();
		}

		ClarifySession IClarifySessionProxy.Session { get { return ClarifySession; } }
	}
}

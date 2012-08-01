using System;
using System.Collections.Generic;
using FChoice.Common.Data;
using FChoice.Foundation.Clarify;
using FChoice.Toolkits.Clarify.FieldOps;
using FChoice.Toolkits.Clarify.Interfaces;
using FChoice.Toolkits.Clarify.Support;

namespace Dovetail.SDK.Bootstrap.Clarify
{
    public interface IApplicationClarifySession : IClarifySession {
    }

    public interface IClarifySession
    {
        Guid Id { get; }
        string UserName { get; }
        int SessionEmployeeID { get; }
        int SessionUserID { get; }
        string SessionEmployeeSiteID { get; }
        ClarifyDataSet CreateDataSet();
        SupportToolkit CreateSupportToolkit();
        FieldOpsToolkit CreateFieldOpsToolkit();
        InterfacesToolkit CreateInterfacesToolkit();
        SqlHelper CreateSqlHelper(string sql);
        string[] Permissions { get; }
    	IEnumerable<string> DataRestriction { get; }
    	void Close();
    }

    public class ClarifySessionWrapper : IApplicationClarifySession
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

	    public DateTime LastLoadTime
	    {
			get { return ClarifySession.LastLoadTime; }
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

        public SqlHelper CreateSqlHelper(string sql)
        {
            return new SqlHelper(sql);
        }

        public void Close()
        {
            ClarifySession.CloseSession();
        }
    }
}
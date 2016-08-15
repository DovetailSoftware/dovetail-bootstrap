using System;
using System.Collections.Generic;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.Clarify
{
    public class ClarifySessionAdapter : IClarifySession, IClarifySessionWrapper, IDisposable
    {
        private readonly ClarifySession _inner;
        private readonly IClarifySessionManager _manager;

        public ClarifySessionAdapter(ClarifySession inner, IClarifySessionManager manager)
        {
            _inner = inner;
            _manager = manager;
        }

        public ClarifySession Clarify { get { return _inner; } }

        public string SessionEmployeeSiteId
        {
            get { return Convert.ToString(Clarify["employee.site.site_id"]); }
        }

        public IEnumerable<string> Permissions
        {
            get { return Clarify.GetAssignedPermissions(); }
        }

        public Guid Id
        {
            get { return Clarify.SessionID; }
        }

        public string UserName
        {
            get { return Clarify.UserName; }
        }

        public int SessionEmployeeId
        {
            get { return Convert.ToInt32(Clarify["employee.id"]); }
        }

        public int SessionUserId
        {
            get { return Convert.ToInt32(Clarify["user.id"]); }
        }

        public IEnumerable<string> DataRestriction
        {
            get
            {
                var restrictionGroup = Clarify.RestrictionGroup;

                return restrictionGroup == null ? new string[0] : restrictionGroup.Restrictions;
            }
        }

        public ClarifyDataSet CreateDataSet()
        {
            return new ClarifyDataSet(Clarify);
        }

        public void RefreshContext()
        {
            Clarify.RefreshContext();
        }

        public void Close()
        {
            ClarifySessionManager.Close(Clarify);
            _manager.Eject(this);
        }

        public void Dispose()
        {
            Close();
        }
    }

}
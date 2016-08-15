using System;
using System.Collections.Generic;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.Clarify
{
    public interface IClarifySession
    {
        Guid Id { get; }
        string UserName { get; }
        int SessionEmployeeId { get; }
        int SessionUserId { get; }
        string SessionEmployeeSiteId { get; }
        IEnumerable<string> Permissions { get; }
        IEnumerable<string> DataRestriction { get; }

        ClarifyDataSet CreateDataSet();
        void RefreshContext();
        void Close();
    }
}
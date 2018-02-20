using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History
{
	public interface IHistoryEmployeeAssembler
	{
		HistoryItemEmployee Assemble(ClarifyDataRow actEntryRecord, IHistoryContactAssembler contactAssembler);
		HistoryItemEmployee Assemble(ClarifyDataRow actEntryRecord);
		ClarifyGeneric TraverseEmployee(ClarifyGeneric actEntryGeneric);
	}

	public class HistoryEmployeeAssembler : IHistoryEmployeeAssembler
	{
		private ClarifyGeneric _userGeneric;
		private ClarifyGeneric _employeeGeneric;
		private readonly IClarifySession _session;

		public HistoryEmployeeAssembler(IClarifySession session)
		{
			_session = session;
		}

		public HistoryItemEmployee Assemble(ClarifyDataRow actEntryRecord, IHistoryContactAssembler contactAssembler)
		{
			var userRows = actEntryRecord.RelatedRows(_userGeneric);
			if (userRows.Length == 0)
				return new HistoryItemEmployee();

			var userRecord = userRows[0];
			var login = userRecord.AsString("login_name");
			var employeeRows = userRecord.RelatedRows(_employeeGeneric);
			if (employeeRows.Length == 0)
				return new HistoryItemEmployee { Login = login };

			var employeeRecord = employeeRows[0];
			var name = "{0} {1}".ToFormat(employeeRecord.AsString("first_name"), employeeRecord.AsString("last_name"));
			var email = employeeRecord.AsString("e_mail");
			var id = employeeRecord.DatabaseIdentifier();

			return new HistoryItemEmployee
			{
				Name = name,
				Firstname = employeeRecord.AsString("first_name"),
				Lastname = employeeRecord.AsString("last_name"),
				Id = id,
				Login = login,
				Email = email,
				PerformedByContact = contactAssembler.Assemble(actEntryRecord)
			};
		}

		public HistoryItemEmployee Assemble(ClarifyDataRow actEntryRecord)
		{
			var proxy = actEntryRecord.AsString("proxy");
			if (proxy.IsEmpty())
				return null;

			var ds = _session.CreateDataSet();
			var generic = ds.CreateGeneric("empl_user");
			generic.Filter(_ => _.Equals("login_name", proxy));
			generic.Query();

			if (generic.Rows.Count == 0)
				return null;

			var employeeRecord = generic.Rows[0];
			var name = "{0} {1}".ToFormat(employeeRecord.AsString("first_name"), employeeRecord.AsString("last_name"));
			var id = employeeRecord.AsInt("employee");

			return new HistoryItemEmployee
			{
				Name = name,
				Firstname = employeeRecord.AsString("first_name"),
				Lastname = employeeRecord.AsString("last_name"),
				Id = id,
				Login = proxy
			};
		}

		public ClarifyGeneric TraverseEmployee(ClarifyGeneric actEntryGeneric)
		{
			_userGeneric = actEntryGeneric.TraverseWithFields("act_entry2user", "objid", "login_name");
			_employeeGeneric = _userGeneric.TraverseWithFields("user2employee", "first_name", "last_name", "e_mail");

			return _employeeGeneric;
		}
	}
}
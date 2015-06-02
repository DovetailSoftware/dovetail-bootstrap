using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History
{
	public interface IHistoryEmployeeAssembler
	{
		HistoryItemEmployee Assemble(ClarifyDataRow actEntryRecord, IHistoryContactAssembler contactAssembler);
		ClarifyGeneric TraverseEmployee(ClarifyGeneric actEntryGeneric);
	}

	public class HistoryEmployeeAssembler : IHistoryEmployeeAssembler
	{
		private ClarifyGeneric _userGeneric;
		private ClarifyGeneric _employeeGeneric;

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

		public ClarifyGeneric TraverseEmployee(ClarifyGeneric actEntryGeneric)
		{
			_userGeneric = actEntryGeneric.TraverseWithFields("act_entry2user", "objid", "login_name");
			_employeeGeneric = _userGeneric.TraverseWithFields("user2employee", "first_name", "last_name", "e_mail");

			return _employeeGeneric;
		}
	}
}
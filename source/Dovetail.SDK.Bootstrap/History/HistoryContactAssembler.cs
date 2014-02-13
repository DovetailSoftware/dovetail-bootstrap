using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History
{
	public interface IHistoryContactAssembler
	{
		HistoryItemContact Assemble(ClarifyDataRow actEntryRecord);
		ClarifyGeneric TraverseContact(ClarifyGeneric actEntryGeneric);
	}

	public class HistoryContactAssembler : IHistoryContactAssembler
	{
		private ClarifyGeneric _contactGeneric;

		public HistoryItemContact Assemble(ClarifyDataRow actEntryRecord)
		{
			var contactRows = actEntryRecord.RelatedRows(_contactGeneric);
			if (contactRows.Length == 0)
				return null;

			var contactRecord = contactRows[0];
			var name = "{0} {1}".ToFormat(contactRecord.AsString("first_namRe"), contactRecord.AsString("last_name"));
			var email = contactRecord.AsString("e_mail");
			var id = contactRecord.DatabaseIdentifier();

			return new HistoryItemContact { Name = name, Id = id, Email = email };
		}

		public ClarifyGeneric TraverseContact(ClarifyGeneric actEntryGeneric)
		{
			_contactGeneric = actEntryGeneric.TraverseWithFields("act_entry2contact", "first_name", "last_name", "e_mail");

			return _contactGeneric;
		}
	}
}
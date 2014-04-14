using System;
using Dovetail.SDK.Bootstrap.History;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.History.Parser;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Schema;
using FChoice.Toolkits.Clarify;
using FChoice.Toolkits.Clarify.Support;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration
{
	[TestFixture]
	public class email_log_updater : MapFixture
	{
		private HistoryItem _historyItem;
		private LogCaseEmailSetup _emailLogSetup;
		private ClarifyDataRow _dataRow;
		private ISchemaCache _schemaCache;
		private int _logEmailObjid;

		public override void beforeAll()
		{
			var mother = new ObjectMother(AdministratorClarifySession);
			var @case = mother.CreateCase();
			_schemaCache = Container.GetInstance<ISchemaCache>();
			_emailLogSetup = new LogCaseEmailSetup(@case.IDNumber)
			{
				Message = "message body",
				CCList = "cc-recipient",
				Recipient = "recipient@example.com"
			};
			_emailLogSetup.AdditionalFields.Append("x_subject", AdditionalFieldType.String, "subject");
			_logEmailObjid = AdministratorClarifySession.CreateSupportToolkit().LogCaseEmail(_emailLogSetup).Objid;
		}

		[SetUp]
		public void beforeEach()
		{
			var dataSet = AdministratorClarifySession.CreateDataSet();
			var emailLogGeneric = dataSet.CreateGeneric("email_log");
			emailLogGeneric.Filter(f => f.Equals("objid", _logEmailObjid));
			emailLogGeneric.Query();

			_dataRow = emailLogGeneric[0];

			_historyItem = new HistoryItem();
			CommonActEntryBuilderDSLExtensions.emailLogUpdater(_dataRow, _historyItem, _schemaCache);
		}

		[Test]
		public void log_should_be_prefixed_with_log_header()
		{
			_historyItem.Detail.ShouldStartWith(HistoryParsers.BEGIN_EMAIL_LOG_HEADER);
		}

		[Test]
		public void log_should_end_with_log_header_then_body()
		{
			_historyItem.Detail.ShouldContain(HistoryParsers.END_EMAIL_LOG_HEADER + _emailLogSetup.Message);
		}

		[Test]
		public void log_should_have_to()
		{
			_historyItem.Detail.ShouldContain("To: {0}".ToFormat(_emailLogSetup.Recipient));
		}

		[Test]
		public void log_should_have_from()
		{
			_historyItem.Detail.ShouldContain("From: {0}".ToFormat(AdministratorClarifySession.UserName));
		}

		[Test]
		public void log_cclist_should_be_present_when_it_is_set()
		{
			_historyItem.Detail.ShouldContain("CC: {0}".ToFormat(_emailLogSetup.CCList));
		}

		[Test]
		public void log_should_have_subject_when_set()
		{
			_historyItem.Detail.ShouldContain("Subject: subject");
		}

		[Test]
		public void log_cclist_should_not_be_present_when_it_is_not_set()
		{
			_dataRow["cc_list"] = DBNull.Value;
			CommonActEntryBuilderDSLExtensions.emailLogUpdater(_dataRow, _historyItem, _schemaCache);

			_historyItem.Detail.Contains("CC:").ShouldBeFalse();
		}

		[Test]
		public void log_should_have_no_subject_when_it_is_not_set()
		{
			_dataRow["x_subject"] = DBNull.Value;
			CommonActEntryBuilderDSLExtensions.emailLogUpdater(_dataRow, _historyItem, _schemaCache);
			
			_historyItem.Detail.Contains("Subject:").ShouldBeFalse();
		}
	}
}
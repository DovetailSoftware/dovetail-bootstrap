using System;
using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Common.Data;
using FChoice.Foundation.Clarify;
using FChoice.Toolkits.Clarify;
using FChoice.Toolkits.Clarify.Interfaces;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Tests.Clarify
{
	public class ObjectMother
	{
		private readonly ClarifySession _session;
		private readonly IList<CleanupInstruction> _instructions;

		public ObjectMother(ClarifyApplication app)
		{
			_session = app.CreateSession("sa", ClarifyLoginType.User);
			_instructions = new List<CleanupInstruction>();
		}

		public string[] GenerateEmployees(int count)
		{
			var ds = new ClarifyDataSet(_session);
			var privGeneric = ds.CreateGeneric("privclass");

			var privClass = Guid.NewGuid().ToString();
			var privClassRow = privGeneric.AddNew();
			privClassRow["class_name"] = privClass;
			privClassRow["access_mask"] = "FFFFFFF FFFF FFFFTFFFFFF FF FF FFFFFF FFF";
			privClassRow["trans_mask"] = new string('1', 255);
			privClassRow["member_type"] = "Employee";
			privClassRow.Update();

			var interfacesToolkit = new InterfacesToolkit(_session);
			var addressObjId = interfacesToolkit.CreateAddress("123 street", "Austin", "TX", "78759", "USA", "CST").Objid;

			var siteSetup = new CreateSiteSetup(SiteType.Customer, SiteStatus.Active, addressObjId)
			{
				SiteIDNum = _session.GetNextNumScheme("Site ID"),
				SiteName = Guid.NewGuid().ToString()
			};

			var siteResult = interfacesToolkit.CreateSite(siteSetup);
			var employees = new List<string>();
			for (var i = 0; i < count; i++)
			{
				var login = "test{0}".ToFormat(Guid.NewGuid().ToString("N").Substring(0, 10));
				var email = "{0}@test.dovetailsoftware.com".ToFormat(login);

				var createEmployeeSetup = new CreateEmployeeSetup("Employee " + i, "Dovetail", login,
					"password", siteResult.IDNum, email, "CSR")
				{
					WorkGroup = "Quality Engineer",
					IsActive = true,
					Phone = randomString(10),
					IsSupervisor = false
				};


				var createEmployeeResult = interfacesToolkit.CreateEmployee(createEmployeeSetup);
				_instructions.Add(new CleanupInstruction("employee", createEmployeeResult.Objid));

				var userId = (int)new SqlHelper("SELECT employee2user FROM table_employee WHERE objid = {0}".ToFormat(createEmployeeResult.Objid)).ExecuteScalar();
				_instructions.Add(new CleanupInstruction("user", userId));

				employees.Add(login);
			}

			_instructions.Add(new CleanupInstruction("site", siteResult.Objid));
			_instructions.Add(new CleanupInstruction("privclass", privClassRow.DatabaseIdentifier()));

			return employees.ToArray();
		}

		private static string randomString(int length)
		{
			return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, length);
		}

		public void CleanUp()
		{
			foreach (var instruction in _instructions)
			{
				var helper = new SqlHelper("DELETE FROM table_{0} WHERE objid = {1}".ToFormat(instruction.Table, instruction.ObjId));
				helper.ExecuteNonQuery();
			}
		}

		private class CleanupInstruction
		{
			public CleanupInstruction(string table, int objId)
			{
				Table = table;
				ObjId = objId;
			}

			public string Table { get; private set; }
			public int ObjId { get; private set; }
		}
	}
}

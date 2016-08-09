using System;
using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation.Clarify;
using FChoice.Toolkits.Clarify;
using FChoice.Toolkits.Clarify.Interfaces;
using FChoice.Toolkits.Clarify.Support;

namespace Dovetail.SDK.ModelMap.Integration
{
	public class ObjectMother
	{
		private readonly IClarifySession _clarifySession;

		public ObjectMother(IClarifySession clarifySession)
		{
			_clarifySession = clarifySession;
		}

		public CaseDTO CreateCase()
		{
			var title = GetRandomString15CharactersLong();

			return CreateCase(title, String.Empty);
		}

		public CaseDTO CreateCase(string title, string description)
		{
			return CreateCase(title, description, "", DateTime.Now);
		}

		public CaseDTO CreateCase(string title, string description, string queue, DateTime createdOn)
		{
			var site = CreateSite();

			var contact = CreateContact(site);

			var createCaseSetup = new CreateCaseSetup(site.SiteIdentifier, contact.FirstName, contact.LastName, contact.Phone)
											{
												Title = GetRandomString15CharactersLong(),
												PhoneLogNotes = description,
												Queue = queue,
												CreateDate = createdOn
											};

			var supportToolkit = _clarifySession.CreateSupportToolkit();
			var createCaseResult = supportToolkit.CreateCase(createCaseSetup);

			return new CaseDTO
						{
							IDNumber = createCaseResult.IDNum,
							ObjID = createCaseResult.Objid,
							Title = createCaseSetup.Title,
							Contact = contact,
							Site = site,
							CreatedOn = createdOn
						};
		}

		public ContactDTO CreateContact(SiteDTO atSite)
		{
			var first = GetRandomString15CharactersLong();
			var last = GetRandomString15CharactersLong();

			return CreateContact(first, last, atSite);
		}

		public ContactDTO CreateContact(string firstName, string lastName, SiteDTO atSite)
		{
			var interfacesToolkit = _clarifySession.CreateInterfacesToolkit();

			var phone = GetRandomString15CharactersLong();

			var createContactSetup = new CreateContactSetup(firstName, lastName, phone, atSite.SiteIdentifier) { Email = firstName + "@example.org" };
			var createContactResult = interfacesToolkit.CreateContact(createContactSetup);

			return new ContactDTO { FirstName = firstName, LastName = lastName, Phone = phone, ObjId = createContactResult.Objid, Site = atSite, Email = createContactSetup.Email };
		}

		public SiteDTO CreateSite()
		{
			var interfacesToolkit = _clarifySession.CreateInterfacesToolkit();

			var addressResult = CreateAddressData();

			var siteSetup = new CreateSiteSetup(SiteType.Customer, SiteStatus.Active, addressResult.Objid)
									{
                                        //LSP violation :O
										SiteIDNum = ((ClarifySessionWrapper) _clarifySession).ClarifySession.GetNextNumScheme("Site ID"),
										SiteName = GetRandomString15CharactersLong()
									};

			var siteResult = interfacesToolkit.CreateSite(siteSetup);

			return new SiteDTO { ObjId = siteResult.Objid, SiteIdentifier = siteSetup.SiteIDNum, Name = siteSetup.SiteName, AddressObjId = addressResult.Objid };
		}

		public EmployeeDTO CreateEmployee()
		{
			return CreateEmployee(true);
		}

		public EmployeeDTO CreateEmployee(bool isActive)
		{
			var firstName = GetRandomString15CharactersLong();
			var lastName = GetRandomString15CharactersLong();

			return CreateEmployee(firstName, lastName, isActive);
		}

		public EmployeeDTO CreateEmployee(string firstName, string lastName, bool isActive)
		{
			var loginName = "l" + firstName;
			var password = "p" + firstName;
			var email = "e" + firstName + "@example.com";

			var site = CreateSite();

			var createEmployeeSetup = new CreateEmployeeSetup(firstName, lastName, loginName, password, site.SiteIdentifier, email, "CSR")
			{
				WorkGroup = "Quality Engineer",
				IsActive = isActive,
				Phone = GetRandomString15CharactersLong()
			};

			var interfacesToolkit = _clarifySession.CreateInterfacesToolkit();
			var createEmployeeResult = interfacesToolkit.CreateEmployee(createEmployeeSetup);

			return new EmployeeDTO
			{
				FirstName = firstName,
				LastName = lastName,
				Login = loginName,
				Email = email,
				ObjId = createEmployeeResult.Objid,
				Password = password,
				Site = site,
				Workgroup = createEmployeeSetup.WorkGroup,
				PrivClass = createEmployeeSetup.OnlinePrivilegeClass,
				Phone = createEmployeeSetup.Phone
			};
		}

		public void CreateQueue(string title)
		{
			var interfacesToolkit = _clarifySession.CreateInterfacesToolkit();
			interfacesToolkit.CreateQueue(title, true, true, true, true, true, true, true, true, true, true);
		}

		private ToolkitResult CreateAddressData()
		{
			var interfacesToolkit = _clarifySession.CreateInterfacesToolkit();

			return interfacesToolkit.CreateAddress("123 street", "Austin", "TX", "78759", "USA", "CST");
		}

		public static string GetRandomString15CharactersLong()
		{
			return Guid.NewGuid().ToString().Substring(0, 15).Replace("-", string.Empty);
		}

		public SolutionDTO CreateSolution()
		{
			var createSolutionSetup = new CreateSolutionSetup
				{
					Title = GetRandomString(),
					Workaround = "first workaround",
					Description = "solution description",
					IsPublic = true,
					CreateDate = DateTime.Now.Subtract(TimeSpan.FromHours(1.01))
				};

			var interfacesToolkit = _clarifySession.CreateInterfacesToolkit();
			var createSolutionResult = interfacesToolkit.CreateSolution(createSolutionSetup);

			var secondWorkaroundResult = interfacesToolkit.AddWorkaround(createSolutionResult.IDNum, "second workaround");

			var workarounds = new[] { createSolutionResult.WorkaroundObjid, secondWorkaroundResult.Objid };

			return new SolutionDTO
						{
							Objid = createSolutionResult.Objid,
							IDNumber = createSolutionResult.IDNum,
							Title = createSolutionSetup.Title,
							Description = createSolutionSetup.Description,
							IsPublic = createSolutionSetup.IsPublic,
							Resolutions = workarounds,
							CreateDate = createSolutionSetup.CreateDate
						};
		}

		private string GetRandomString()
		{
			return Guid.NewGuid().ToString().Replace("-", "");
		}

		public void AddSiteToContact(SiteDTO siteDto, ContactDTO contactDto)
		{
			var interfacesToolkit = _clarifySession.CreateInterfacesToolkit();

			interfacesToolkit.UpdateContact(contactDto.FirstName, contactDto.LastName, contactDto.Phone, siteDto.SiteIdentifier, "");
		}
	}

	public class SolutionDTO
	{
		public int Objid { get; set; }
		public string IDNumber { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public bool IsPublic { get; set; }
		public DateTime CreateDate { get; set; }
		public int[] Resolutions { get; set; }
	}

	public class EmployeeDTO
	{
		public int ObjId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Workgroup { get; set; }
		public string PrivClass { get; set; }

		public SiteDTO Site { get; set; }
	}

	public static class TestDTOExtensions
	{
		public static void AddToQueue(this EmployeeDTO employee, string queueName, ClarifySession clarifySession)
		{
			new InterfacesToolkit(clarifySession).AddUserToQueue(queueName, employee.Login);
		}

		public static void LinkCaseToFirstSolutionResolution(this SolutionDTO solution, CaseDTO @case, ClarifySession clarifySession)
		{
			new InterfacesToolkit(clarifySession).LinkCaseToWorkaround(@case.IDNumber, solution.Resolutions[0]);
		}

		public static void Yank(this EmployeeDTO employee, CaseDTO @case, ClarifySession clarifySession)
		{
			new SupportToolkit(clarifySession).YankCase(new YankCaseSetup(@case.IDNumber) { UserName = employee.Login });
		}

		public static void Assign(this CaseDTO @case, EmployeeDTO employee, ClarifySession clarifySession)
		{
			new SupportToolkit(clarifySession).AssignCase(new AssignCaseSetup(@case.IDNumber) { UserName = employee.Login });
		}

		public static void Dispatch(this CaseDTO @case, string queueName, ClarifySession clarifySession)
		{
			new SupportToolkit(clarifySession).DispatchCase(new DispatchCaseSetup(@case.IDNumber, queueName));
		}

		public static void Dispatch(this EmployeeDTO employee, CaseDTO @case, string queueName, ClarifySession clarifySession)
		{
			new SupportToolkit(clarifySession).DispatchCase(new DispatchCaseSetup(@case.IDNumber, queueName) { UserName = employee.Login });
		}

		public static void AddNotes(this CaseDTO @case, int numberOfNotes, ClarifySession clarifySession)
		{
			var supportToolkit = new SupportToolkit(clarifySession);

			for (var i = 0; i < numberOfNotes; i++)
			{
				supportToolkit.LogCaseNote(new LogCaseNoteSetup(@case.IDNumber) { Notes = "case notes" });
			}
		}
	}

	public class SiteDTO
	{
		public int ObjId { get; set; }
		public string Name { get; set; }
		public string SiteIdentifier { get; set; }
		public int AddressObjId { get; set; }
	}

	public class ContactDTO
	{
		public string Email { get; set; }
		public int ObjId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Phone { get; set; }

		public SiteDTO Site { get; set; }
	}

	public class CaseDTO
	{
		public int ObjID { get; set; }
		public string IDNumber { get; set; }
		public string Title { get; set; }
		public DateTime CreatedOn { get; set; }
		public SiteDTO Site { get; set; }
		public ContactDTO Contact { get; set; }
	}
}

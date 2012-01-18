using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.ModelMap.Registration;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration
{
    public class ContactRole
    {
        public int ContactRoleDatabaseIdentifier { get; set; }
        public string Role { get; set; }
        public bool IsPrimaryRole { get; set; }
        public string SiteName { get; set; }
        public string SiteId { get; set; }
    }

	public class TableToViewWithManyDTOs
	{
		public int CustomerDatabaseIdentifier { get; set; }
		public string CustomerName { get; set; }
		public string CustomerPhone { get; set; }
		public IEnumerable<ContactRole> Roles { get; set; }
	}

	public class TableToViewWithDTOsMap : ModelMap<TableToViewWithManyDTOs>
	{
		protected override void MapDefinition()
		{
			FromTable("contact")
				.Assign(d => d.CustomerDatabaseIdentifier).FromIdentifyingField("objid")
				.Assign(d => d.CustomerName).FromFields("first_name", "last_name")
				.Assign(d => d.CustomerPhone).FromField("phone")
                //.MapMany<ContactRole>().To(s => s.Roles)
                //    .ViaAdhocRelation("objid", "rol_contct", "con_objid", view => view
                //        .SortAscendingBy("primary_site")
                //        .Assign(d => d.ContactRoleDatabaseIdentifier).FromField("objid")
                //        .Assign(d => d.Role).FromField("role_name")
                //        .Assign(d => d.IsPrimaryRole).BasedOnField("primary_site").Do(primary_site => primary_site == "1")
                //        .Assign(d => d.SiteName).FromField("site")
                //        .Assign(d => d.SiteId).FromField("site_id")
					;
		}
	}

	public class mapping_from_table_to_view_with_many_dtos
	{
		[TestFixture]
		[Explicit]  // need to add ad-hoc support for MapMany
		public class getting_many : MapFixture
		{
			private TableToViewWithManyDTOs _viewModel;
			private ContactDTO _contact;
			private SiteDTO _site;

			public override void beforeAll()
			{
				_site = new ObjectMother(AdministratorClarifySession).CreateSite();
				_contact = new ObjectMother(AdministratorClarifySession).CreateContact(_site);

				var assembler = Container.GetInstance<IModelBuilder<TableToViewWithManyDTOs>>();
				_viewModel = assembler.GetOne(_contact.ObjId);
			}

			[Test]
			public void should_return_contact_information()
			{
				_viewModel.CustomerDatabaseIdentifier.ShouldEqual(_contact.ObjId);
				_viewModel.CustomerName.ShouldStartWith(_contact.FirstName);
				_viewModel.CustomerName.ShouldEndWith(_contact.LastName);
				_viewModel.CustomerPhone.ShouldEqual(_contact.Phone);
			}

			[Test]
			public void should_return_contact_role()
			{
				_viewModel.Roles.First().ContactRoleDatabaseIdentifier.ShouldEqual(_contact.ObjId);
			}

			[Test]
			public void should_return_all_contact_roles()
			{
				var newSite = new ObjectMother(AdministratorClarifySession).CreateSite();
				new ObjectMother(AdministratorClarifySession).AddSiteToContact(newSite, _contact);

				_viewModel.Roles.Count().ShouldEqual(2);

				var primaryContactRole = _viewModel.Roles.ElementAt(0);
				primaryContactRole.IsPrimaryRole.ShouldBeTrue();
				primaryContactRole.SiteId.ShouldEqual(_site.SiteIdentifier);
				primaryContactRole.SiteName.ShouldEqual(_site.Name);

				_viewModel.Roles.ElementAt(1).IsPrimaryRole.ShouldBeFalse();

				var secondaryContactRole = _viewModel.Roles.ElementAt(1);
				secondaryContactRole.IsPrimaryRole.ShouldBeFalse();
				secondaryContactRole.SiteId.ShouldEqual(newSite.SiteIdentifier);
				secondaryContactRole.SiteName.ShouldEqual(newSite.Name);
			}
		}
	}
}
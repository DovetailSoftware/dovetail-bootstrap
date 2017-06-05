using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.ModelMap.Legacy;
using Dovetail.SDK.ModelMap.Legacy.Registration;
using FChoice.Foundation.Filters;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.Legacy
{
	public class ViewToTableToView
	{
		public string CaseTitle { get; set; }
		public string CaseId { get; set; }
		public string SiteName { get; set; }
	}

	public class ViewToTableToViewMap : ModelMap<ViewToTableToView>
	{
		protected override void MapDefinition()
		{
			FromView("fc_user_case_view")
				.Assign(d => d.CaseId).FromField("id_number")
				.ViaAdhocRelation("id_number", "case", "id_number", g => g
					.Assign(d => d.CaseTitle).FromField("title")
					.ViaAdhocRelation("objid", "fc_user_case_view", "case_objid", map => map
						.Assign(d => d.SiteName).FromField("site_name")
						)
				);
		}
	}

	public class mapping_from_view_to_table_to_view
	{
		[TestFixture]
		public class getting_many : MapFixture
		{
			private IEnumerable<ViewToTableToView> _viewModels;
			private IEnumerable<CaseDTO> _cases;

			public override void beforeAll()
			{
			    var objectMother = new ObjectMother(AdministratorClarifySession);
			    _cases = new[] { objectMother.CreateCase(), objectMother.CreateCase() };
				var assembler = Container.GetInstance<IModelBuilder<ViewToTableToView>>();
				_viewModels = assembler.Get(FilterType.IsIn("id_number", _cases.Select(c => c.IDNumber).ToArray()));
			}

			[Test]
			public void should_return_all_matching_case_titles()
			{
				_cases.Select(s => s.Title).ShouldHaveMatchingContents(_viewModels.Select(v => v.CaseTitle));
			}

			[Test]
			public void should_return_all_requested_case_ids()
			{
				_cases.Select(s => s.IDNumber).ShouldHaveMatchingContents(_viewModels.Select(v => v.CaseId));
			}

			[Test]
			public void should_return_all_requested_site()
			{
				_cases.Select(s => s.Site.Name).ShouldHaveMatchingContents(_viewModels.Select(v => v.SiteName));
			}
		}
	}

	public class ViewToTableModel
	{
		public string CaseTitle { get; set; }
		public string CaseId { get; set; }
		public string SiteName { get; set; }
	}

	public class ViewToTableMap : ModelMap<ViewToTableModel>
	{
		protected override void MapDefinition()
		{
			FromView("fc_user_case_view")
				.Assign(d => d.CaseId).FromField("id_number")
				.ViaAdhocRelation("case_objid", "case", "objid", g => g
					.Assign(d => d.CaseTitle).FromField("title")
					.ViaRelation("case_reporter2site", site => site
						.Assign(d => d.SiteName).FromField("name")
						)
				);
		}
	}

	public class mapping_from_view_to_table 
	{ 
		[TestFixture]
		public class getting_many : MapFixture
		{
			private IEnumerable<ViewToTableModel> _viewModels;
			private IEnumerable<CaseDTO> _cases;

			public override void beforeAll()
			{
                var objectMother = new ObjectMother(AdministratorClarifySession);
                _cases = new[] { objectMother.CreateCase(), objectMother.CreateCase() };
				var assembler = Container.GetInstance<IModelBuilder<ViewToTableModel>>();
				_viewModels = assembler.Get(FilterType.IsIn("id_number", _cases.Select(c=>c.IDNumber).ToArray()));
			}

			[Test]
			public void should_return_all_matching_case_titles()
			{
				_cases.Select(s => s.Title).ShouldHaveMatchingContents(_viewModels.Select(v => v.CaseTitle));
			}
			
			[Test]
			public void should_return_all_requested_case_ids()
			{
				_cases.Select(s => s.IDNumber).ShouldHaveMatchingContents(_viewModels.Select(v => v.CaseId));
			}

			[Test]
			public void should_return_all_requested_site()
			{
				_cases.Select(s => s.Site.Name).ShouldHaveMatchingContents(_viewModels.Select(v => v.SiteName));
			}
		}

		[TestFixture]
		public class getting_one : MapFixture
		{
			private ViewToTableModel _viewToTableModel;
			private CaseDTO _case;

			public override void beforeAll()
			{
				base.beforeAll();

				_case = new ObjectMother(AdministratorClarifySession).CreateCase();
				var assembler = Container.GetInstance<IModelBuilder<ViewToTableModel>>();

				_viewToTableModel = assembler.Get(FilterType.Equals("id_number", _case.IDNumber)).First();
			}

			[Test]
			public void view_fields_should_be_assigned()
			{
				_viewToTableModel.CaseId.ShouldEqual(_case.IDNumber);
			}

			[Test]
			public void site_related_to_case_should_be_populated()
			{
				_viewToTableModel.SiteName.ShouldEqual(_case.Site.Name);
			}

			[Test]
			public void case_linked_to_view_should_be_populated()
			{
				_viewToTableModel.CaseTitle.ShouldEqual(_case.Title);
			}
		}
	}
}
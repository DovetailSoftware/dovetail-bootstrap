using System.Linq;
using Dovetail.SDK.ModelMap.Registration;
using FChoice.Foundation.Filters;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration
{
	[TestFixture]
	public class Assembling_paginated_dtos : MapFixture
	{
		private SolutionDTO _solution1Dto;
		private SolutionDTO _solution2Dto;
		private IModelBuilder<Solution> _solutionAssembler;

		public override void beforeAll()
		{
			base.beforeAll();

			_solution1Dto = new ObjectMother(AdministratorClarifySession).CreateSolution();
			_solution2Dto = new ObjectMother(AdministratorClarifySession).CreateSolution();

			Container.Configure(d => d.For<ModelMap<Solution>>().Use<SolutionIdentifiedByIdNumberMap>());
		}

		[SetUp]
		public void beforeEach()
		{
			_solutionAssembler = Container.GetInstance<IModelBuilder<Solution>>();
		}

		[Test]
		public void requesting_a_page_should_populate_models_and_pagination_result()
		{
			var results = _solutionAssembler.Get(f => f.IsIn("objid", _solution1Dto.Objid, _solution2Dto.Objid), new PaginationRequest {PageSize = 1, CurrentPage = 1});

			results.Models.Count().ShouldEqual(1);
			results.Models.First().IdNumber.ShouldEqual(_solution1Dto.IDNumber);

			results.CurrentPage.ShouldEqual(1);
			results.PageSize.ShouldEqual(1);
			results.TotalRecordCount.ShouldEqual(2);
		}

		[Test]
		public void requesting_the_second_page()
		{
			var results = _solutionAssembler.Get(f => f.IsIn("objid", _solution1Dto.Objid, _solution2Dto.Objid), new PaginationRequest {PageSize = 1, CurrentPage = 2});

			results.Models.Count().ShouldEqual(1);
			results.Models.First().IdNumber.ShouldEqual(_solution2Dto.IDNumber);
			
			results.CurrentPage.ShouldEqual(2);	
			results.PageSize.ShouldEqual(1);
			results.TotalRecordCount.ShouldEqual(2);
		}

		[Test]
		public void requesting_multi_item_page()
		{
			var results = _solutionAssembler.Get(f => f.IsIn("objid", _solution1Dto.Objid, _solution2Dto.Objid), new PaginationRequest { PageSize = 10, CurrentPage = 1 });

			results.Models.Count().ShouldEqual(2);
			results.Models.First().IdNumber.ShouldEqual(_solution1Dto.IDNumber);
			results.Models.Skip(1).First().IdNumber.ShouldEqual(_solution2Dto.IDNumber);

			results.CurrentPage.ShouldEqual(1);
			results.PageSize.ShouldEqual(10);
			results.TotalRecordCount.ShouldEqual(2);
		}

		[Test]
		//This is an ANTI-TEST. Under scoring a limitation of Dovetial SDK
		public void requesting_multi_item_page_when_only_one_result_is_possible_does_not_populate_total_record_count()
		{
			var results = _solutionAssembler.Get(f => f.IsIn("objid", _solution1Dto.Objid), new PaginationRequest { PageSize = 2, CurrentPage = 1 });
			
			results.TotalRecordCount.ShouldEqual(0);
		}

		[Test]
		public void requesting_pages_outside_the_possible_results_should_be_empty()
		{
			var results = _solutionAssembler.Get(f => f.IsIn("objid", _solution1Dto.Objid, _solution2Dto.Objid), new PaginationRequest {PageSize = 1, CurrentPage = 3});

			results.Models.Count().ShouldEqual(0);
		}
	}

	[TestFixture]
	public class Assembling_dtos : MapFixture
	{
		private SolutionDTO _solution1Dto;
		private SolutionDTO _solution2Dto;

		public override void beforeAll()
		{
			base.beforeAll();

			_solution1Dto = new ObjectMother(AdministratorClarifySession).CreateSolution();
			_solution2Dto = new ObjectMother(AdministratorClarifySession).CreateSolution();

			Container.Configure(d => d.For<ModelMap<Solution>>().Use<SolutionIdentifiedByIdNumberMap>());
		}

		[Test]
		public void using_a_filter()
		{
			var solutionAssembler = Container.GetInstance<IModelBuilder<Solution>>();

			var solutions = solutionAssembler.Get(FilterType.Equals("title", _solution2Dto.Title));

			solutions.Single().IdNumber.ShouldEqual(_solution2Dto.IDNumber);
		}

		[Test]
		public void identifying_string_fields_are_used_when_getting_one_from_the_assembler()
		{
			Container.Configure(d => d.For<ModelMap<Solution>>().Use<SolutionIdentifiedByIdNumberMap>());
			var solutionAssembler = Container.GetInstance<IModelBuilder<Solution>>();

			var solution = solutionAssembler.GetOne(_solution1Dto.IDNumber);
			
			solution.Title.ShouldEqual(_solution1Dto.Title);
		}

		[Test]
		public void identifying_integer_fields_are_used_when_getting_one_from_the_assembler()
		{
			Container.Configure(d => d.For<ModelMap<Solution>>().Use<SolutionIdentifiedByObjIdMap>());
			var solutionAssembler = Container.GetInstance<IModelBuilder<Solution>>();

			var solution = solutionAssembler.GetOne(_solution1Dto.Objid);

			solution.Title.ShouldEqual(_solution1Dto.Title);
		}
	}

	public class SolutionIdentifiedByObjIdMap : ModelMap<Solution>
	{
		protected override void MapDefinition()
		{
			FromTable("probdesc")
				.Assign(d => d.DatabaseIdentifier).FromIdentifyingField("objid")
				.Assign(d => d.Title).FromField("title")
				.SortAscendingBy("id_number");
		}
	}

	public class Solution
	{
		public int DatabaseIdentifier { get; set; }
		public string IdNumber { get; set; }
		public string Title { get; set; }
	}

	public class SolutionIdentifiedByIdNumberMap : ModelMap<Solution>
	{
		protected override void MapDefinition()
		{
			FromTable("probdesc")
				.Assign(d => d.IdNumber).FromIdentifyingField("id_number")
				.Assign(d => d.Title).FromField("title");
		}
	}
}
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
		public void results_are_limited_to_the_requested_page()
		{
			var results = _solutionAssembler.Get(f => f.IsIn("objid", _solution1Dto.Objid, _solution2Dto.Objid), new Pagination {PageSize = 1, CurrentPage = 1});

			results.Results.Count().ShouldEqual(1);
			results.Results.First().IdNumber.ShouldEqual(_solution1Dto.IDNumber);

			results.Pagination.CurrentPage.ShouldEqual(1);
			results.Pagination.PageSize.ShouldEqual(1);
			results.Pagination.TotalCount.ShouldEqual(2);
		}

		[Test]
		public void second_page()
		{
			var results = _solutionAssembler.Get(f => f.IsIn("objid", _solution1Dto.Objid, _solution2Dto.Objid), new Pagination {PageSize = 1, CurrentPage = 2});

			results.Results.Count().ShouldEqual(1);
			results.Results.First().IdNumber.ShouldEqual(_solution2Dto.IDNumber);

			results.Pagination.CurrentPage.ShouldEqual(2);
			results.Pagination.PageSize.ShouldEqual(1);
			results.Pagination.TotalCount.ShouldEqual(2);
		}

		[Test]
		public void pages_past_the_possible_results_should_be_empty()
		{
			var results = _solutionAssembler.Get(f => f.IsIn("objid", _solution1Dto.Objid, _solution2Dto.Objid), new Pagination {PageSize = 1, CurrentPage = 3});

			results.Results.Count().ShouldEqual(0);
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
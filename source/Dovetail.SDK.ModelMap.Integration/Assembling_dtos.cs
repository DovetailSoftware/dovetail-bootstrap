using System.Linq;
using Dovetail.SDK.ModelMap.Registration;
using FChoice.Foundation.Filters;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration
{
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
		public void top_dtos_for_a_filter_only_the_requested_number_of_dtos_should_be_returned()
		{
			var solutionAssembler = Container.GetInstance<IModelBuilder<Solution>>();

			var solutions = solutionAssembler.GetTop(FilterType.NotEqual(("objid"), -100), 1);

			solutions.Count().ShouldEqual(1);
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

		public class SolutionIdentifiedByIdNumberMap : ModelMap<Solution>
		{
			protected override void MapDefinition()
			{
				FromTable("probdesc")
					.Assign(d => d.IdNumber).FromIdentifyingField("id_number")
					.Assign(d => d.Title).FromField("title");
			}
		}

		public class SolutionIdentifiedByObjIdMap : ModelMap<Solution>
		{
			protected override void MapDefinition()
			{
				FromTable("probdesc")
					.Assign(d => d.DatabaseIdentifier).FromIdentifyingField("objid")
					.Assign(d => d.Title).FromField("title");
			}
		}

		public class Solution
		{
			public int DatabaseIdentifier { get; set; }
			public string IdNumber { get; set; }
			public string Title { get; set; }
		}
	}
}
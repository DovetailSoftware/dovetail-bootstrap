using Dovetail.SDK.ModelMap.Registration;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration
{
	[TestFixture]
	public class Filtering_a_child_generic : MapFixture
	{
		private IModelBuilder<FilteredSolution> _solutionAssembler;
		private SolutionDTO _solutionDto;
		private FilteredSolution _solution;

		public override void beforeAll()
		{
			base.beforeAll();

			_solutionDto = new ObjectMother(AdministratorClarifySession).CreateSolution();

			//put a service into the container to inject the solution's resolution objid
			Container.Inject(new FilterInjectionService { FilterObjid = _solutionDto.Resolutions[1] });
			_solutionAssembler = Container.GetInstance<IModelBuilder<FilteredSolution>>();
			_solution = _solutionAssembler.GetOne(_solutionDto.IDNumber);
		}

		[Test]
		public void should_apply_filter_to_child_generic()
		{
			_solution.Resolutions.Length.ShouldEqual(_solutionDto.Resolutions.Length-1);
			_solution.Resolutions[0].DatabaseIdentifier.ShouldEqual(_solutionDto.Resolutions[1]);
		}

		public class FilteredSolutionMap : ModelMap<FilteredSolution>
		{
			private readonly FilterInjectionService _service;

			public FilteredSolutionMap(FilterInjectionService service)
			{
				_service = service;
			}

			protected override void MapDefinition()
			{
				FromTable("probdesc")
					.Assign(d => d.SolutionID).FromIdentifyingField("id_number")
					.MapMany<Resolution>().To(d => d.Resolutions).ViaRelation("probdesc2workaround", workaround => workaround
						.Assign(d => d.DatabaseIdentifier).FromField("objid")
						.FilteredBy(f => f.Equals("objid", _service.FilterObjid))
					);
			}
		}

		public class FilteredSolution
		{
			public string SolutionID { get; set; }
			public Resolution[] Resolutions { get; set; }
		}

		public class Resolution
		{
			public int DatabaseIdentifier { get; set; }
			public string Title { get; set; }
		}

		public class User
		{
			public string Name { get; set; }
		}
	}

	public class FilterInjectionService
	{
		public int FilterObjid { get; set; }
	}
}
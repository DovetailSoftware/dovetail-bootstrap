using System.Linq;
using Dovetail.SDK.ModelMap.Legacy;
using Dovetail.SDK.ModelMap.Legacy.Registration;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.Legacy
{
	[TestFixture]
	public class Mapping_many_dtos_to_a_field : MapFixture
	{
		private IModelBuilder<Solution> _solutionAssembler;
		private SolutionDTO _solutionDto;
		private Solution _solution;

		public override void beforeAll()
		{
			base.beforeAll();

			_solutionDto = new ObjectMother(AdministratorClarifySession).CreateSolution();
			_solutionAssembler = Container.GetInstance<IModelBuilder<Solution>>();
			_solution = _solutionAssembler.GetOne(_solutionDto.IDNumber);
		}

		[Test]
		public void should_still_populate_regular_fields()
		{
			_solution.SolutionID.ShouldEqual(_solutionDto.IDNumber);
		}

		[Test]
		public void many_map_should_populate_resolutions()
		{
			_solution.Resolutions.Length.ShouldEqual(_solutionDto.Resolutions.Length);

			_solution.Resolutions.First().ShouldBeOfType(typeof(Resolution));

			_solution.Resolutions.First().DatabaseIdentifier.ShouldEqual(_solutionDto.Resolutions.First());
            _solution.Resolutions.ElementAt(1).DatabaseIdentifier.ShouldEqual(_solutionDto.Resolutions.ElementAt(1));
		}

		[Test]
		public void support_map_one_relationships()
		{
			_solution.CreatedBy.ShouldBeOfType(typeof(User));
			_solution.CreatedBy.Name.ShouldEqual("sa");
		}

		[Test]
		public void supports_multiple_relationships()
		{
			_solution.OwnedBy.ShouldBeOfType(typeof(User));
			_solution.OwnedBy.Name.ShouldEqual("sa");
		}

		public class SolutionMap : ModelMap<Solution>
		{
			protected override void MapDefinition()
			{
				FromTable("probdesc")
					.Assign(d => d.SolutionID).FromIdentifyingField("id_number")
					.MapMany<Resolution>().To(d => d.Resolutions).ViaRelation("probdesc2workaround", workaround => workaround
						.Assign(d => d.DatabaseIdentifier).FromField("objid")
					)                    
					.MapOne<User>().To(d => d.CreatedBy).ViaRelation("probdesc_orig2user", workaround => workaround
					    .Assign(d => d.Name).FromField("login_name")
					)
					.MapOne<User>().To(d => d.OwnedBy).ViaRelation("probdesc_owner2user", workaround => workaround
						.Assign(d => d.Name).FromField("login_name")
					);
			}
		}

		public class Solution
		{
			public string SolutionID { get; set; }
			public Resolution[] Resolutions { get; set; }
			public User CreatedBy{ get; set; }
			public User OwnedBy { get; set; }
		}

		public class Resolution
		{
			public int DatabaseIdentifier { get; set; }
		}

		public class User
		{
			public string Name{ get; set; }
		}
	}
}
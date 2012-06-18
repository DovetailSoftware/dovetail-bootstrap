using System;
using System.Linq;
using Dovetail.SDK.ModelMap.Registration;
using FChoice.Foundation.Filters;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration
{
	[TestFixture]
	public class Mapping_from_table : MapFixture
	{
		public static DateTime ExpectedFromFunctionDateTime = DateTime.Now.AddYears(-3);

		private SolutionDTO _solutionDto;
		private Solution _solution;

		public override void beforeAll()
		{
			base.beforeAll();

			_solutionDto = new ObjectMother(AdministratorClarifySession).CreateSolution();
			var solutionAssembler = Container.GetInstance<IModelBuilder<Solution>>();
			_solution = solutionAssembler.Get(FilterType.Equals("id_number", _solutionDto.IDNumber)).First();
		}

		[Test]
		public void assigning_from_field_should_map_values_from_database()
		{
			_solution.Title.ShouldEqual(_solutionDto.Title);
			_solution.Created.ShouldBeClose(_solutionDto.CreateDate.ToUniversalTime());
			_solution.DatabaseIdentifier.ShouldEqual(_solutionDto.Objid);
		}

		[Test]
		public void assigning_from_identiying_field_should_map_values_from_database()
		{
			_solution.ID.ShouldEqual(_solutionDto.IDNumber);
        }

		[Test]
		public void assigning_multiple_fields_to_a_property_should_concatenate_the_fields_delimited_by_spaces()
		{
			_solution.MultiField.ShouldEqual(_solutionDto.Objid + " " + _solutionDto.Title);
		}

		[Test]
		public void assigning_based_on_a_field_should_use_lambda_to_transform_the_database_field_value()
		{
			_solution.IsPublic.ShouldEqual(_solutionDto.IsPublic);
		}

		[Test]
		public void assigning_from_function_should_put_the_function_output_into_the_dto_property()
		{
			_solution.IntFromFunction.ShouldEqual(ExpectedFromFunctionDateTime.Hour);
		}
		
		public class SolutionMap : ModelMap<Solution>
		{
			protected override void MapDefinition()
			{
				FromTable("probdesc")
					.Assign(d => d.ID).FromIdentifyingField("id_number")
					.Assign(d => d.Title).FromField("title")
					.Assign(d => d.Created).FromField("creation_time")
					.Assign(d => d.DatabaseIdentifier).FromField("objid")
					.Assign(d => d.MultiField).FromFields("objid", "title")
					.Assign(d => d.IsPublic).BasedOnField("public_ind").Do(isPublic => isPublic == "1")
					.Assign(d => d.IntFromFunction).FromFunction(() => ExpectedFromFunctionDateTime.Hour);
			}
		}

		public class Solution
		{
			public int DatabaseIdentifier { get; set; }
			public string ID { get; set; }
			public DateTime Created { get; set; }
			public string Title { get; set; }
			public string MultiField { get; set; }
			public bool IsPublic { get; set; }
			public int IntFromFunction { get; set; }
		}
	}
}
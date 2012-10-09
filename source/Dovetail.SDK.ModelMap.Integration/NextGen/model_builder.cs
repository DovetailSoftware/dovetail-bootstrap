using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.ModelMap.NextGen;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NextGen
{
	public class CaseModel
	{
		public string Title { get; set; }
		public string Id { get; set; }
	}

	[TestFixture]
	public class model_builder : MapFixture
	{
		private IEnumerable<CaseModel> _result;
		private CaseDTO _case;

		public override void beforeAll()
		{
			var objectMother = new ObjectMother(AdministratorClarifySession);

			_case = objectMother.CreateCase();

			var mapFactory = Container.GetInstance<IModelMapConfigFactory<CaseModel, CaseModel>>();

			var map = mapFactory.Create("case", c =>
				{
					c.SelectField("title", f => f.Title);
					c.SelectField("id_number", f => f.Id).EqualTo(f=>f.Id);
				});

			Container.Configure(c=> c.For<ModelMapConfig<CaseModel, CaseModel>>().Use(map));

			var modelBuilder = Container.GetInstance<IModelBuilder<CaseModel, CaseModel>>();

			_result = modelBuilder.Execute(new CaseModel {Id = _case.IDNumber});
		}

		[Test]
		public void found_id()
		{
			_result.First().Id.ShouldEqual(_case.IDNumber);
		}

		[Test]
		public void found_title()
		{
			_result.First().Title.ShouldEqual(_case.Title);
		}

	}
}
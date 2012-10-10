using System.Linq;
using Dovetail.SDK.ModelMap.NextGen;
using FChoice.Foundation.Schema;
using NUnit.Framework;
using Rhino.Mocks;
using StructureMap;

namespace Dovetail.SDK.ModelMap.Integration.NextGen
{
	public class TestModel
	{
		public string Title { get; set; }
		public string Site { get; set; }
		public object Id { get; set; }
	}

	public class InputModel	
	{
		public string SiteName { get; set; }
	}

	[TestFixture]
	public class next_gen 
	{
		[Test]
		public void look_see()
		{
			var schemaCache = MockRepository.GenerateStub<ISchemaCache>();
			var container = MockRepository.GenerateStub<IContainer>();

			var mapFactory = new ModelMapFactory<InputModel, TestModel>(container, schemaCache);

			var map = mapFactory.Create("case", c =>
				{
					c.SelectField("title", f => f.Title);
					c.SelectField("id_number", f => f.Id);
					c.Join("case_reporter2site", site => site.SelectField("name", f => f.Site).EqualTo(input => input.SiteName));

					//c.Join("case_tag", tag =>
					//    {
					//        tag.SelectField("name", f => f.Site).EqualTo(input => input.SiteName);
					//        tag.Join("tag2user", user => user.Field("login_name").EqualTo(c.Get<ICurrentSDKUser>().Username));
					//    });
				});

			var builder = new ModelBuilder<InputModel, TestModel>(map);

			var outputModels = builder.Execute(new InputModel {SiteName = "site name"});

			outputModels.First().Title.ShouldEqual("case title");
		}
	}
}
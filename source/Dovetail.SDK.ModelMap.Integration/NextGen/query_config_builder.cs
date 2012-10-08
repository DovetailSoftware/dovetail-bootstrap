using System.Linq;
using Dovetail.SDK.ModelMap.NextGen;
using FChoice.Foundation.Schema;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NextGen
{
	public class TestModel
	{
		public string Title { get; set; }
		public string SiteName { get; set; }
		public string SiteiD { get; set; }
		public int SiteStatus { get; set; }
	}

	public class FilterModel	
	{
		public string SiteName { get; set; }
	}

	[TestFixture]
	public class query_config_builder : MapFixture
	{
		private MapQueryConfig _query;
		private ISchemaCache _schemaCache;
		private FilterModel _filter;

		public override void beforeAll()
		{
			_schemaCache = Container.GetInstance<ISchemaCache>();
			var mapFactory = new ModelMapFactory<FilterModel, TestModel>(Container, _schemaCache);

			var map = mapFactory.Create("case", c =>
			{
				c.SelectField("title", f => f.Title);

				c.Join("case_reporter2site", site =>
				{
					site.Field("status").EqualTo(42);
					site.SelectField("name", f => f.SiteName).EqualTo(input => input.SiteName);
					site.SelectField("site_id", f => f.SiteiD);
				});
			});

			var builder = new MapQueryConfigFactory<FilterModel, TestModel>(map);

			_filter = new FilterModel { SiteName = "site name" };

			_query = builder.Create(_filter);
		}

		[Test]
		public void root_selected_field()
		{
			var title = _query.Selects.First(s => s.Field.Name == "title");
			title.Alias.ShouldEqual("root");
			title.Field.ShouldEqual(_schemaCache.GetField("case","title"));
		}

		[Test]
		public void selected_field_with_constraint()
		{
			var name = _query.Selects.First(s => s.Field.Name == "name");
			name.Alias.ShouldEqual("T0");
			name.Field.ShouldEqual(_schemaCache.GetField("site", "name"));

			_query.Wheres.Any(s => s.Field.Name == "name").ShouldBeTrue();
		}

		[Test]
		public void selected_field()
		{
			var siteId = _query.Selects.First(s => s.Field.Name == "site_id");
			siteId.Alias.ShouldEqual("T0");
			siteId.Field.ShouldEqual(_schemaCache.GetField("site", "site_id"));
		}
		
		[Test]
		public void field_with_where_constrained_by_an_object()
		{
			_query.Selects.Any(s => s.Field.Name == "status").ShouldBeFalse();

			var status = _query.Wheres.First(s => s.Field.Name == "status");
			status.Alias.ShouldEqual("T0");
			status.Field.ShouldEqual(_schemaCache.GetField("site", "status"));
			status.Value.ShouldEqual(42);		
		}

		[Test]
		public void field_with_where_constrained_by_an_input_property()
		{
			var status = _query.Wheres.First(s => s.Field.Name == "name");
			status.Alias.ShouldEqual("T0");
			status.Field.ShouldEqual(_schemaCache.GetField("site", "name"));
			status.Value.ShouldEqual(_filter.SiteName);
		}

	}
}
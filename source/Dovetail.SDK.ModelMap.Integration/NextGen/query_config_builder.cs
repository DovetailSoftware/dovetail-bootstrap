using System;
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
		public DateTime? Modified { get; set; }
		public int ObjId { get; set; }
	}

	[TestFixture]
	public class model_map_factory_no_selects : MapFixture
	{
		private ISchemaCache _schemaCache;
		private ModelMapConfigFactory<FilterModel, TestModel> _mapFactory;

		public override void beforeAll()
		{
			_schemaCache = Container.GetInstance<ISchemaCache>();
			_mapFactory = new ModelMapConfigFactory<FilterModel, TestModel>(Container, _schemaCache);
		}

		[Test]
		public void should_throw_when_no_fields_are_selected()
		{
			typeof (DovetailMappingException).ShouldBeThrownBy(() => _mapFactory.Create("case", c => c.Field("objid").FilterableBy(filter => filter.ObjId)));
		}
	}


	[TestFixture]
	public class model_map_factory_filterable_fields : MapFixture
	{
		private ISchemaCache _schemaCache;
		private RootModelMapConfig<FilterModel, TestModel> _map;

		public override void beforeAll()
		{
			_schemaCache = Container.GetInstance<ISchemaCache>();
			var mapFactory = new ModelMapConfigFactory<FilterModel, TestModel>(Container, _schemaCache);

			_map = mapFactory.Create("case", c =>
				{
					c.SelectField("title", f => f.Title);

					c.Join("case_reporter2site", site =>
						{
							site.Field("update_stamp").FilterableBy(filter => filter.Modified);
							site.SelectField("name", f=>f.SiteName).EqualTo(f=>f.SiteName);
						});
				});
		}

		[Test]
		public void should_find_filterable_property()
		{
			_map.SetFilter(f=>f.Modified).ShouldNotBeNull();
		}

		[Test]
		public void should_find_filtered_property()
		{
			_map.SetFilter(f => f.SiteName).ShouldNotBeNull();
		}

		[Test]
		public void should_throw_when_property_is_not_a_configured_filter()
		{
			typeof (DovetailMappingException).ShouldBeThrownBy(() => _map.SetFilter(f => f.ObjId));
		}
	}

	[TestFixture]
	public class map_query_config_factory : MapFixture
	{
		private MapQueryConfig _query;
		private ISchemaCache _schemaCache;
		private FilterModel _filter;

		public override void beforeAll()
		{
			_schemaCache = Container.GetInstance<ISchemaCache>();
			var mapFactory = new ModelMapConfigFactory<FilterModel, TestModel>(Container, _schemaCache);

			var map = mapFactory.Create("case", c =>
			{
				c.SelectField("title", f => f.Title);

				c.Join("case_reporter2site", site =>
				{
					site.Field("status").EqualTo(42);
					site.Field("region");
					site.SelectField("name", f => f.SiteName).EqualTo(filter => filter.SiteName);
					site.SelectField("site_id", f => f.SiteiD);
					site.Field("update_stamp").FilterableBy(filter => filter.Modified);
				});
			});

			var builder = new MapQueryFactory<FilterModel, TestModel>(map);

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
		public void future_filter_adds_field_config_to_root_map_config()
		{
			var title = _query.Selects.First(s => s.Field.Name == "title");
			title.Alias.ShouldEqual("root");
			title.Field.ShouldEqual(_schemaCache.GetField("case", "title"));
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

			var where = _query.Wheres.First(s => s.Field.Name == "status");
			where.Alias.ShouldEqual("T0");
			where.Field.ShouldEqual(_schemaCache.GetField("site", "status"));
			where.Value.ShouldEqual(42);		
		}

		[Test]
		public void field_with_where_constrained_by_an_input_property()
		{
			var where = _query.Wheres.First(s => s.Field.Name == "name");
			where.Alias.ShouldEqual("T0");
			where.Field.ShouldEqual(_schemaCache.GetField("site", "name"));
			where.Operator.ShouldBeOfType(typeof (EqualsFilterOperator));
			where.Value.ShouldEqual(_filter.SiteName);
		}

		[Test]
		public void field_with_no_filter_specified_should_have_no_where_item()
		{
			_query.Wheres.Any(s => s.Field.Name == "region").ShouldBeFalse();
		}

		[Test]
		public void field_with_no_filter_value_should_have_no_where_item()
		{
			_query.Wheres.Any(s => s.Field.Name == "update_stamp").ShouldBeFalse();
		}
	}
}
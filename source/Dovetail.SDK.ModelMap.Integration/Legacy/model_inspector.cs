using Dovetail.SDK.Bootstrap.History;
using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Integration;
using Dovetail.SDK.ModelMap.Legacy;
using Dovetail.SDK.ModelMap.Legacy.Registration;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.Legacy
{
	[TestFixture]
	public class model_inspector_for_string_based_identifier : MapFixture
	{
		private ModelMapFieldDetails result;

		[SetUp]
		public void beforeEach()
		{
			var inspector = Container.GetInstance<ModelInspector<Kase>>();
			result = inspector.GetIdentifier();
		}

		[Test]
		public void should_find_identifier_field()
		{
			result.FieldName.ShouldEqual("id_number");
		}

		[Test]
		public void should_return_schema_type()
		{
			result.SchemaFieldType.ShouldEqual(typeof(string));
		}

		[Test]
		public void should_return_model_property()
		{
			result.Property.ShouldEqual(typeof(Kase).GetProperty("Id"));
		}
	}

	[TestFixture]
	public class model_inspector_for_int_based_identifier : MapFixture
	{
		private ModelMapFieldDetails result;

		[SetUp]
		public void beforeEach()
		{
			var inspector = Container.GetInstance<ModelInspector<Kontact>>();
			result = inspector.GetIdentifier();
		}

		[Test]
		public void should_find_identifier_field()
		{
			result.FieldName.ShouldEqual("objid");
		}

		[Test]
		public void should_return_schema_type()
		{
			result.SchemaFieldType.ShouldEqual(typeof(int));
		}

		[Test]
		public void should_return_model_property()
		{
			result.Property.ShouldEqual(typeof(Kontact).GetProperty("Id"));
		}
	}

	[TestFixture]
	public class model_inspector_for_type_without_identifier : MapFixture
	{
		private ModelMapFieldDetails result;

		[SetUp]
		public void beforeEach()
		{
			var inspector = Container.GetInstance<ModelInspector<KontactNoId>>();
			result = inspector.GetIdentifier();
		}

		[Test]
		public void result_should_be_null()
		{
			result.ShouldBeNull();
		}
	}


	public class KaseMap : ModelMap<Kase>
	{
		protected override void MapDefinition()
		{
			FromView("fc_user_case_view")
				.SortDescendingBy("modify_stmp")
				.Assign(d => d.Id).FromIdentifyingField("id_number")
				.Assign(d => d.Type).FromFunction(() => WorkflowObject.Case);
		}
	}

	public class Kase
	{
		public string Id { get; set; }
		public string Type { get; set; }
	}


	public class KontactMap : ModelMap<Kontact>
	{
		protected override void MapDefinition()
		{
			FromTable("contact")
				.Assign(d => d.Id).FromIdentifyingField("objid")
				.Assign(d => d.Dev).FromField("dev");
		}
	}

	public class Kontact
	{
		public int Id { get; set; }
		public string Dev { get; set; }
	}


	public class Kontact2Map : ModelMap<KontactNoId>
	{
		protected override void MapDefinition()
		{
			FromTable("contact")
				.Assign(d => d.Id).FromField("objid")
				.Assign(d => d.Dev).FromField("dev");
		}
	}

	public class KontactNoId
	{
		public int Id { get; set; }
		public string Dev { get; set; }
	}
}
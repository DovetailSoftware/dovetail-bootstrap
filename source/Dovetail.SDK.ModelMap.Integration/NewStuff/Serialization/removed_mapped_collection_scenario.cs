using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff.Serialization
{
	[TestFixture]
	public class removed_mapped_collection_scenario
	{
		private ModelMapParsingScenario theScenario;

		[SetUp]
		public void SetUp()
		{
			theScenario = ModelMapParsingScenario.Create(_ =>
			{
				_.UseFile("advanced-case.map.config");
				_.UseFile("removed-mapped-collection.map.config");
				_.UseFile("site.partial.config");
			});
		}

		[Test]
		public void verify_instructions()
		{
			VerifyInstructions.Assert(theScenario.Instructions, _ =>
			{
				_.Get<BeginModelMap>().Name.ShouldEqual("test");
				_.Get<BeginView>().ViewName.ShouldEqual("qry_case_view");
				_.Get<BeginProperty>().Key.ShouldEqual("id");
				_.Get<EndProperty>();

				_.Get<BeginProperty>().Key.ShouldEqual("title");
				_.Get<EndProperty>();

				_.Get<BeginProperty>().Key.ShouldEqual("ownerUsername");
				_.Get<EndProperty>();

				_.Get<BeginProperty>().Key.ShouldEqual("caseType");
				_.Get<BeginTransform>().Name.ShouldEqual("localizedListItem");
				_.Get<AddTransformArgument>().Name.ShouldEqual("listName");
				_.Get<AddTransformArgument>().Name.ShouldEqual("listValue");
				_.Get<EndTransform>();
				_.Get<EndProperty>();

				_.Verify<BeginAdHocRelation>(__ =>
				{
					__.FromTableField.ShouldEqual("elm_objid");
					__.ToTableName.ShouldEqual("case");
					__.ToTableFieldName.ShouldEqual("objid");
				});

				_.Get<BeginProperty>().Key.ShouldEqual("id");
				_.Get<EndProperty>();

				_.Get<BeginProperty>().Key.ShouldEqual("lastModified");
				_.Get<EndProperty>();

				_.Verify<BeginRelation>(__ => __.RelationName.ShouldEqual("case_currq2queue"));
				_.Get<BeginProperty>().Key.ShouldEqual("inQueue");
				_.Get<EndProperty>();
				_.Get<EndRelation>();

				_.Is<PushVariableContext>();
				_.Get<BeginMappedProperty>().Key.ShouldEqual("site");
				_.Verify<BeginRelation>(__ => __.RelationName.ShouldEqual("${relationName}"));
				_.Get<BeginProperty>().Key.ShouldEqual("id");
				_.Get<EndProperty>();
				_.Get<EndRelation>();
				_.Get<EndMappedProperty>();
				_.Is<PopVariableContext>();

				_.Get<EndRelation>();

				_.Get<EndView>();
				_.Get<EndModelMap>();
			});

			theScenario.Instructions.Length.ShouldEqual(34);
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}
using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff.Serialization
{
	[TestFixture]
	public class remove_nested_property_scenario
	{
		private ModelMapParsingScenario theScenario;

		[SetUp]
		public void SetUp()
		{
			theScenario = ModelMapParsingScenario.Create(_ =>
			{
				_.UseFile("advanced-case.map.config");
				_.UseFile("removed-nested-property.map.config");
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

				_.Get<BeginMappedCollection>().Key.ShouldEqual("attachments");
				_.Verify<BeginRelation>(__ => __.RelationName.ShouldEqual("case_attch2doc_inst"));
				_.Verify<FieldSortMap>(__ =>
				{
					__.Field.ShouldEqual("objid");
					__.Type.ShouldEqual("desc");
				});

				_.Get<BeginProperty>().Key.ShouldEqual("title");
				_.Get<EndProperty>();

				_.Verify<BeginRelation>(__ => __.RelationName.ShouldEqual("attach_info2doc_path"));
				_.Get<BeginProperty>().Key.ShouldEqual("fileIcon");
				_.Get<EndProperty>();
				_.Get<EndRelation>();

				_.Verify<BeginRelation>(__ => __.RelationName.ShouldEqual("doc_inst2act_entry"));
				_.Get<BeginProperty>().Key.ShouldEqual("uploaded");
				_.Get<EndProperty>();
				_.Get<BeginMappedProperty>().Key.ShouldEqual("uploader");
				_.Verify<BeginRelation>(__ => __.RelationName.ShouldEqual("act_entry2user"));
				_.Get<BeginProperty>().Key.ShouldEqual("login");
				_.Get<EndProperty>();
				_.Verify<BeginRelation>(__ => __.RelationName.ShouldEqual("user2employee"));
				_.Get<BeginProperty>().Key.ShouldEqual("firstName");
				_.Get<EndProperty>();
				_.Get<BeginProperty>().Key.ShouldEqual("lastName");
				_.Get<EndProperty>();
				_.Get<EndRelation>();
				_.Get<EndRelation>();
				_.Get<EndMappedProperty>();
				_.Get<EndRelation>();

				_.Get<EndRelation>();
				_.Get<EndMappedCollection>();
				_.Get<EndRelation>();

				_.Get<EndView>();
				_.Get<EndModelMap>();
			});

			theScenario.Instructions.Length.ShouldEqual(61);
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}
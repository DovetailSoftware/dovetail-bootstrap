using Dovetail.SDK.ModelMap.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.Serialization
{
	[TestFixture]
	public class add_nested_mapped_property_scenario
	{
		private ModelMapParsingScenario theScenario;

		[SetUp]
		public void SetUp()
		{
			theScenario = ModelMapParsingScenario.Create(_ =>
			{
				_.UseFile("advanced-case.map.config");
				_.UseFile("add-nested-mapped-property.map.config");
				_.UseFile("site.partial.config");
			});
		}

		[Test]
		public void verify_instructions()
		{
			VerifyInstructions.Assert(theScenario.Instructions, _  =>
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
				_.Is<EndProperty>();

				_.Verify<BeginMappedProperty>(__ => __.Key.ShouldEqual("currentQueue"));
				_.Verify<BeginRelation>(__ => __.RelationName.ShouldEqual("case_currq2queue"));
				_.Get<BeginProperty>().Key.ShouldEqual("id");
				_.Get<EndProperty>();
				_.Get<BeginProperty>().Key.ShouldEqual("title");
				_.Get<EndProperty>();
				_.Get<EndRelation>();
				_.Get<EndMappedProperty>();

				_.Get<BeginProperty>().Key.ShouldEqual("lastModified");
				_.Is<EndProperty>();


				_.Is<PushVariableContext>();
				_.Get<BeginMappedProperty>().Key.ShouldEqual("site");
				_.Verify<BeginRelation>(__ => __.RelationName.ShouldEqual("${relationName}"));
				_.Get<BeginProperty>().Key.ShouldEqual("id");
				_.Is<EndProperty>();
				_.Is<EndRelation>();
				_.Is<EndMappedProperty>();
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

				_.Get<BeginProperty>().Key.ShouldEqual("id");
				_.Get<EndProperty>();

				_.Verify<BeginRelation>(__ => __.RelationName.ShouldEqual("attach_info2doc_path"));
				_.Get<BeginProperty>().Key.ShouldEqual("fileIcon");
				_.Is<EndProperty>();
				_.Is<EndRelation>();

				_.Verify<BeginRelation>(__ => __.RelationName.ShouldEqual("doc_inst2act_entry"));
				_.Get<BeginProperty>().Key.ShouldEqual("uploaded");
				_.Is<EndProperty>();
				_.Get<BeginMappedProperty>().Key.ShouldEqual("uploader");
				_.Verify<BeginRelation>(__ => __.RelationName.ShouldEqual("act_entry2user"));
				_.Get<BeginProperty>().Key.ShouldEqual("login");
				_.Is<EndProperty>();
				_.Verify<BeginRelation>(__ => __.RelationName.ShouldEqual("user2employee"));
				_.Get<BeginProperty>().Key.ShouldEqual("firstName");
				_.Is<EndProperty>();
				_.Get<BeginProperty>().Key.ShouldEqual("lastName");
				_.Is<EndProperty>();
				_.Is<EndRelation>();
				_.Is<EndRelation>();
				_.Get<EndMappedProperty>();
				_.Is<EndRelation>();

				_.Is<EndRelation>();
				_.Is<EndMappedCollection>();
				_.Is<EndRelation>();

				_.Is<EndView>();
				_.Is<EndModelMap>();
			});
			

			theScenario.Instructions.Length.ShouldEqual(67);
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}
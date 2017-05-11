using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff.Serialization
{
	[TestFixture]
	public class add_traversed_property_scenario
	{
		private ModelMapParsingScenario theScenario;

		[SetUp]
		public void SetUp()
		{
			theScenario = ModelMapParsingScenario.Create(_ =>
			{
				_.UseFile("advanced-case.map.config");
				_.UseFile("add-traversed-property.map.config");
				_.UseFile("site.partial.config");
			});
		}

		[Test]
		public void verify_instructions()
		{
			theScenario.Get<BeginModelMap>(0).Name.ShouldEqual("test");
			theScenario.Get<BeginView>(1).ViewName.ShouldEqual("qry_case_view");
			theScenario.Get<BeginProperty>(2).Key.ShouldEqual("id");
			theScenario.Get<EndProperty>(3);

			theScenario.Get<BeginProperty>(4).Key.ShouldEqual("title");
			theScenario.Get<EndProperty>(5);

			theScenario.Get<BeginProperty>(6).Key.ShouldEqual("ownerUsername");
			theScenario.Get<EndProperty>(7);

			theScenario.Get<BeginProperty>(8).Key.ShouldEqual("caseType");
			theScenario.Get<BeginTransform>(9).Name.ShouldEqual("localizedListItem");
			theScenario.Get<AddTransformArgument>(10).Name.ShouldEqual("listName");
			theScenario.Get<AddTransformArgument>(11).Name.ShouldEqual("listValue");
			theScenario.Get<EndTransform>(12);
			theScenario.Get<EndProperty>(13);

			theScenario.Verify<BeginAdHocRelation>(14, _ =>
			{
				_.FromTableField.ShouldEqual("elm_objid");
				_.ToTableName.ShouldEqual("case");
				_.ToTableFieldName.ShouldEqual("objid");
			});

			theScenario.Get<BeginProperty>(15).Key.ShouldEqual("newProperty");
			theScenario.Get<EndProperty>(16);

			theScenario.Get<BeginProperty>(17).Key.ShouldEqual("id");
			theScenario.Get<EndProperty>(18);

			theScenario.Get<BeginProperty>(19).Key.ShouldEqual("lastModified");
			theScenario.Get<EndProperty>(20);

			theScenario.Verify<BeginRelation>(21, _ => _.RelationName.ShouldEqual("case_currq2queue"));
			theScenario.Get<BeginProperty>(22).Key.ShouldEqual("inQueue");
			theScenario.Get<EndProperty>(23);
			theScenario.Get<EndRelation>(24);

			theScenario.Get<BeginMappedProperty>(25).Key.ShouldEqual("site");
			theScenario.Verify<BeginRelation>(26, _ => _.RelationName.ShouldEqual("case_reporter2site"));
			theScenario.Get<BeginProperty>(27).Key.ShouldEqual("id");
			theScenario.Get<EndProperty>(28);
			theScenario.Get<EndRelation>(29);
			theScenario.Get<EndMappedProperty>(30);

			theScenario.Get<BeginMappedCollection>(31).Key.ShouldEqual("attachments");
			theScenario.Verify<BeginRelation>(32, _ => _.RelationName.ShouldEqual("case_attch2doc_inst"));
			theScenario.Verify<FieldSortMap>(33, _ =>
			{
				_.Field.ShouldEqual("objid");
				_.Type.ShouldEqual("desc");
			});

			theScenario.Get<BeginProperty>(34).Key.ShouldEqual("title");
			theScenario.Get<EndProperty>(35);

			theScenario.Get<BeginProperty>(36).Key.ShouldEqual("id");
			theScenario.Get<EndProperty>(37);

			theScenario.Verify<BeginRelation>(38, _ => _.RelationName.ShouldEqual("attach_info2doc_path"));
			theScenario.Get<BeginProperty>(39).Key.ShouldEqual("fileIcon");
			theScenario.Get<EndProperty>(40);
			theScenario.Get<EndRelation>(41);

			theScenario.Verify<BeginRelation>(42, _ => _.RelationName.ShouldEqual("doc_inst2act_entry"));
			theScenario.Get<BeginProperty>(43).Key.ShouldEqual("uploaded");
			theScenario.Get<EndProperty>(44);
			theScenario.Get<BeginMappedProperty>(45).Key.ShouldEqual("uploader");
			theScenario.Verify<BeginRelation>(46, _ => _.RelationName.ShouldEqual("act_entry2user"));
			theScenario.Get<BeginProperty>(47).Key.ShouldEqual("login");
			theScenario.Get<EndProperty>(48);
			theScenario.Verify<BeginRelation>(49, _ => _.RelationName.ShouldEqual("user2employee"));
			theScenario.Get<BeginProperty>(50).Key.ShouldEqual("firstName");
			theScenario.Get<EndProperty>(51);
			theScenario.Get<BeginProperty>(52).Key.ShouldEqual("lastName");
			theScenario.Get<EndProperty>(53);
			theScenario.Get<EndRelation>(54);
			theScenario.Get<EndRelation>(55);
			theScenario.Get<EndMappedProperty>(56);
			theScenario.Get<EndRelation>(57);

			theScenario.Get<EndRelation>(58);
			theScenario.Get<EndMappedCollection>(59);
			theScenario.Get<EndRelation>(60);

			theScenario.Get<EndView>(61);
			theScenario.Get<EndModelMap>(62);

			theScenario.Instructions.Length.ShouldEqual(63);
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}
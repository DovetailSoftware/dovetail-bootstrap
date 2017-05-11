using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff.Serialization
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

			theScenario.Get<BeginProperty>(15).Key.ShouldEqual("id");
			theScenario.Get<EndProperty>(16);

			theScenario.Verify<BeginMappedProperty>(17, _ => _.Key.ShouldEqual("currentQueue"));
			theScenario.Verify<BeginRelation>(18, _ => _.RelationName.ShouldEqual("case_currq2queue"));
			theScenario.Get<BeginProperty>(19).Key.ShouldEqual("id");
			theScenario.Get<EndProperty>(20);
			theScenario.Get<BeginProperty>(21).Key.ShouldEqual("title");
			theScenario.Get<EndProperty>(22);
			theScenario.Get<EndRelation>(23);
			theScenario.Get<EndMappedProperty>(24);

			theScenario.Get<BeginProperty>(25).Key.ShouldEqual("lastModified");
			theScenario.Get<EndProperty>(26);

			

			theScenario.Get<BeginMappedProperty>(27).Key.ShouldEqual("site");
			theScenario.Verify<BeginRelation>(28, _ => _.RelationName.ShouldEqual("case_reporter2site"));
			theScenario.Get<BeginProperty>(29).Key.ShouldEqual("id");
			theScenario.Get<EndProperty>(30);
			theScenario.Get<EndRelation>(31);
			theScenario.Get<EndMappedProperty>(32);

			theScenario.Get<BeginMappedCollection>(33).Key.ShouldEqual("attachments");
			theScenario.Verify<BeginRelation>(34, _ => _.RelationName.ShouldEqual("case_attch2doc_inst"));
			theScenario.Verify<FieldSortMap>(35, _ =>
			{
				_.Field.ShouldEqual("objid");
				_.Type.ShouldEqual("desc");
			});

			theScenario.Get<BeginProperty>(36).Key.ShouldEqual("title");
			theScenario.Get<EndProperty>(37);

			theScenario.Get<BeginProperty>(38).Key.ShouldEqual("id");
			theScenario.Get<EndProperty>(39);

			theScenario.Verify<BeginRelation>(40, _ => _.RelationName.ShouldEqual("attach_info2doc_path"));
			theScenario.Get<BeginProperty>(41).Key.ShouldEqual("fileIcon");
			theScenario.Get<EndProperty>(42);
			theScenario.Get<EndRelation>(43);

			theScenario.Verify<BeginRelation>(44, _ => _.RelationName.ShouldEqual("doc_inst2act_entry"));
			theScenario.Get<BeginProperty>(45).Key.ShouldEqual("uploaded");
			theScenario.Get<EndProperty>(46);
			theScenario.Get<BeginMappedProperty>(47).Key.ShouldEqual("uploader");
			theScenario.Verify<BeginRelation>(48, _ => _.RelationName.ShouldEqual("act_entry2user"));
			theScenario.Get<BeginProperty>(49).Key.ShouldEqual("login");
			theScenario.Get<EndProperty>(50);
			theScenario.Verify<BeginRelation>(51, _ => _.RelationName.ShouldEqual("user2employee"));
			theScenario.Get<BeginProperty>(52).Key.ShouldEqual("firstName");
			theScenario.Get<EndProperty>(53);
			theScenario.Get<BeginProperty>(54).Key.ShouldEqual("lastName");
			theScenario.Get<EndProperty>(55);
			theScenario.Get<EndRelation>(56);
			theScenario.Get<EndRelation>(57);
			theScenario.Get<EndMappedProperty>(58);
			theScenario.Get<EndRelation>(59);

			theScenario.Get<EndRelation>(60);
			theScenario.Get<EndMappedCollection>(61);
			theScenario.Get<EndRelation>(62);

			theScenario.Get<EndView>(63);
			theScenario.Get<EndModelMap>(64);

			theScenario.Instructions.Length.ShouldEqual(65);
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}
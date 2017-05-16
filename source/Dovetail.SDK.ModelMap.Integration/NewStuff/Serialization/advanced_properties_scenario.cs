﻿using Dovetail.SDK.ModelMap.NewStuff.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NewStuff.Serialization
{
	[TestFixture]
	public class advanced_properties_scenario
	{
		private ModelMapParsingScenario theScenario;

		[SetUp]
		public void SetUp()
		{
			theScenario = ModelMapParsingScenario.Create(_ =>
			{
				_.UseFile("advanced-case.map.config");
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

			theScenario.Get<BeginProperty>(17).Key.ShouldEqual("lastModified");
			theScenario.Get<EndProperty>(18);

			theScenario.Verify<BeginRelation>(19, _ => _.RelationName.ShouldEqual("case_currq2queue"));
			theScenario.Get<BeginProperty>(20).Key.ShouldEqual("inQueue");
			theScenario.Get<EndProperty>(21);
			theScenario.Get<EndRelation>(22);

			theScenario.Get<BeginMappedProperty>(23).Key.ShouldEqual("site");
			theScenario.Verify<BeginRelation>(24, _ => _.RelationName.ShouldEqual("case_reporter2site"));
			theScenario.Get<BeginProperty>(25).Key.ShouldEqual("id");
			theScenario.Get<EndProperty>(26);
			theScenario.Get<EndRelation>(27);
			theScenario.Get<EndMappedProperty>(28);

			theScenario.Get<BeginMappedCollection>(29).Key.ShouldEqual("attachments");
			theScenario.Verify<BeginRelation>(30, _ => _.RelationName.ShouldEqual("case_attch2doc_inst"));
			theScenario.Verify<FieldSortMap>(31, _ =>
			{
				_.Field.ShouldEqual("objid");
				_.Type.ShouldEqual("desc");
			});

			theScenario.Get<BeginProperty>(32).Key.ShouldEqual("title");
			theScenario.Get<EndProperty>(33);

			theScenario.Get<BeginProperty>(34).Key.ShouldEqual("id");
			theScenario.Get<EndProperty>(35);

			theScenario.Verify<BeginRelation>(36, _ => _.RelationName.ShouldEqual("attach_info2doc_path"));
			theScenario.Get<BeginProperty>(37).Key.ShouldEqual("fileIcon");
			theScenario.Get<EndProperty>(38);
			theScenario.Get<EndRelation>(39);

			theScenario.Verify<BeginRelation>(40, _ => _.RelationName.ShouldEqual("doc_inst2act_entry"));
			theScenario.Get<BeginProperty>(41).Key.ShouldEqual("uploaded");
			theScenario.Get<EndProperty>(42);
			theScenario.Get<BeginMappedProperty>(43).Key.ShouldEqual("uploader");
			theScenario.Verify<BeginRelation>(44, _ => _.RelationName.ShouldEqual("act_entry2user"));
			theScenario.Get<BeginProperty>(45).Key.ShouldEqual("login");
			theScenario.Get<EndProperty>(46);
			theScenario.Verify<BeginRelation>(47, _ => _.RelationName.ShouldEqual("user2employee"));
			theScenario.Get<BeginProperty>(48).Key.ShouldEqual("firstName");
			theScenario.Get<EndProperty>(49);
			theScenario.Get<BeginProperty>(50).Key.ShouldEqual("lastName");
			theScenario.Get<EndProperty>(51);
			theScenario.Get<EndRelation>(52);
			theScenario.Get<EndRelation>(53);
			theScenario.Get<EndMappedProperty>(54);
			theScenario.Get<EndRelation>(55);

			theScenario.Get<EndRelation>(56);
			theScenario.Get<EndMappedCollection>(57);
			theScenario.Get<EndRelation>(58);

			theScenario.Get<EndView>(59);
			theScenario.Get<EndModelMap>(60);

			theScenario.Instructions.Length.ShouldEqual(61);
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}
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

			theScenario.Get<EndRelation>(29);

			theScenario.Get<EndView>(30);
			theScenario.Get<EndModelMap>(31);

			theScenario.Instructions.Length.ShouldEqual(32);
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}
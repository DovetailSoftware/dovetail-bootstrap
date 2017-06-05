using System.Linq;
using FChoice.Foundation.Clarify;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.Legacy
{
	[TestFixture]
	public class List_values_from_hgbst_list : MapFixture
	{
        private IListCache _listCache;

		public override void beforeAll()
		{
			base.beforeAll();

            _listCache = Container.GetInstance<IListCache>();
		}

		[Test]
		public void should_include_top_level_items()
		{
			const string listTitle = "CASE_TYPE";

            var hgbstElmDefaultElement = _listCache.GetHgbstElmDefaultElement(listTitle);

			var hgbstListItems = _listCache.GetHgbstList(listTitle, null);

			hgbstListItems.ElementAt(hgbstElmDefaultElement.Rank).Title.ShouldEqual(hgbstElmDefaultElement.Title);
		}

		[Test]
		public void should_include_second_level_items_for_specified_top_level_item()
		{
			const string listTitle = "CASE_TYPE";
			const string level1Title = "Marketing Information";

			var hgbstList = _listCache.GetHgbstList(listTitle, new [] { level1Title});

			hgbstList.Count().ShouldEqual(1);
			hgbstList.First().Title.ShouldEqual("Please Specify");
		}

		[Test]
		public void should_include_third_level_items_when_the_top_two_levels_are_specified()
		{
			const string listTitle = "CR_DESC";
			const string level1Title = "PC";
			const string level2Title = "Windows 3.1";

			var hgbstList = _listCache.GetHgbstList(listTitle, new[] { level1Title, level2Title });

			hgbstList.Count().ShouldEqual(2);
			hgbstList.ElementAt(0).Title.ShouldEqual("8m");
			hgbstList.ElementAt(1).Title.ShouldEqual("16m");
		}
	}
}
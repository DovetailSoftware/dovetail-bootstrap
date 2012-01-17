using System.Collections.Generic;
using System.Linq;
using FChoice.Foundation.Clarify;
using FubuCore;
using FubuCore.Reflection;
using FubuMVC.Core.UI.Configuration;
using HtmlTags;

namespace Dovetail.SDK.Fubu.Clarify.Lists
{
    public class GbstListValueDropdownBuilder : ElementBuilder
    {
        protected override bool matches(AccessorDef def)
        {
            var hasAttribute = def.Accessor.HasAttribute<GbstListValueAttribute>();

            return hasAttribute;
        }

        public override HtmlTag Build(ElementRequest request)
        {
            return new SelectTag(tag =>
            {
                buildOptions(request, tag);
                tag.AddClass("form-list");
            });
        }

        private static void buildOptions(ElementRequest request, SelectTag tag)
        {
            var listElements = GetListElementTitlesOrderedByRank(request);

            listElements.ElementTitles.Each(title => tag.Option(title, title));

            var requestValue = request.Value<string>();

            var defaultValue = requestValue.IsNotEmpty() ? requestValue : listElements.DefaultElementTitle;

            tag.SelectByValue(defaultValue);
        }

        private static ListElements GetListElementTitlesOrderedByRank(ElementRequest request)
        {
            var att = request.Accessor.GetAttribute<GbstListValueAttribute>();
            var listName = att.GetListName();

            var listCache = request.Get<IListCache>();
            var gbstList = listCache.GetGbstList(listName);

            var elementTitles = gbstList.ActiveElements.OrderBy(e => e.Rank).Select(element => element.Title);

            return new ListElements { DefaultElementTitle = gbstList.DefaultElement.Title, ElementTitles = elementTitles };
        }

        private class ListElements
        {
            public string DefaultElementTitle { get; set; }
            public IEnumerable<string> ElementTitles { get; set; }
        }
    }

}
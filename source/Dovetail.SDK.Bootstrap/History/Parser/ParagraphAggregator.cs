using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History.Parser
{
	public interface IParagraphAggregator
	{
		IEnumerable<IItem> CollapseContentItems(IEnumerable<IItem> items);
	}

	public class Paragraph : IItem, IRenderHtml
	{
		public IEnumerable<Line> Lines { get; set; }

		public override string ToString()
		{
			return String.Join("\n", Lines.Select(l => l.Text));
		}

		public string RenderHtml()
		{
			return "<p>" + String.Join("<br/>", Lines.Select(l => l.Text)) + "</p>";
		}
	}

	public class ParagraphAggregator : IParagraphAggregator
	{
		public IEnumerable<IItem> CollapseContentItems(IEnumerable<IItem> items)
		{
			var output = new List<IItem>();
			var content = new List<Line>();

			items.Each(i =>
			{
				var itemType = i.GetType();

				//collect line items for possible collapsing into a paragraph
				if (itemType.CanBeCastTo<Line>())
				{
					content.Add(i as Line);
					return;
				}

				if (itemType.CanBeCastTo<ParagraphEnd>())
				{
					if (content.Count == 0) return; //ignore paragraph ends when there is no leading lines

					//collapse collected lines when a paragraph end is found
					output.Add(new Paragraph {Lines = content.ToArray()});
					content.Clear();
					return;
				}

				// before adding non paragraph items clear out the collected content
				if (content.Count > 0) 
				{
					output.AddRange(content);
					content.Clear();
				}

				output.Add(i);
			});

			output.AddRange(content);

			return output;
		}
	}
}
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
			var lines = new List<Line>();

			items.Each(i =>
			{
				var itemType = i.GetType();

				//collect line items for possible collapsing into a paragraph
				if (itemType.CanBeCastTo<Line>())
				{
					lines.Add(i as Line);
					return;
				}

				createParagraphIfPossible(output, lines);

				//paragraph ends should be added to the output stream
				if (itemType.CanBeCastTo<ParagraphEnd>()) return;
				
				//when the item has nested items we need to collapse those too
				if (itemType.CanBeCastTo<IHasNestedItems>())
				{
					var originalMessage = (IHasNestedItems)i;
					originalMessage.Items = CollapseContentItems(originalMessage.Items);
				}

				//add item to the non Line item to output
				output.Add(i);
			});

			//make sure any collected lines get added to the output
			createParagraphIfPossible(output, lines);

			return output;
		}

		private void createParagraphIfPossible(List<IItem> output, List<Line> lines)
		{
			if (lines.Count == 0) return; //do nothing when there are no llines collected

			//collapse collected lines into a paragraph
			output.Add(new Paragraph { Lines = lines.ToArray() });
			lines.Clear();
		}
	}
}
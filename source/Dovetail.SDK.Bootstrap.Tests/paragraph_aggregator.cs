using System.Linq;
using Dovetail.SDK.Bootstrap.History.Parser;
using NUnit.Framework;

namespace Dovetail.SDK.Bootstrap.Tests
{
	[TestFixture]
	public class paragraph_aggregator
	{
		private ParagraphAggregator _cut;

		[SetUp]
		public void beforeEach()
		{
			_cut = new ParagraphAggregator();
		}


		[Test]
		public void empty_input_should_have_empty_output()
		{
			var items = new IItem[0];

			var results = _cut.CollapseContentItems(items).ToArray();

			results.Length.ShouldEqual(0);
		}

		[Test]
		public void lines_seperated_by_non_paragraph_items_should_be_standalone()
		{
			var items = new IItem[]
			{
				new OriginalMessage {Items = new IItem[0]}, 
				new Line {Text = "p1l1"}, 
				new OriginalMessage {Items = new IItem[0]}, 
				new Line {Text = "p1l2"}, 
				new OriginalMessage {Items = new IItem[0]} 
			};

			var results = _cut.CollapseContentItems(items).ToArray();

			results.Length.ShouldEqual(5);
			results[0].ShouldBeOfType<OriginalMessage>();
			results[1].ShouldBeOfType<Line>();
			results[2].ShouldBeOfType<OriginalMessage>();
			results[3].ShouldBeOfType<Line>();
			results[4].ShouldBeOfType<OriginalMessage>();
		}

		[Test]
		public void should_collapse_lines_followed_by_a_paragraph_end_into_a_paragraph()
		{
			var items = new IItem[]
			{
				new Line {Text = "p1l1"}, 
				new Line {Text = "p1l2"}, 
				new ParagraphEnd(),
			};

			var results = _cut.CollapseContentItems(items).ToArray();

			results.Length.ShouldEqual(1);
			results.First().ShouldBeOfType<Paragraph>();

			var paragraph = (Paragraph) results.First();

			var lines = paragraph.Lines.ToArray();
			lines.Length.ShouldEqual(2);
			lines[0].Text.ShouldEqual(items[0].ToString());
			lines[1].Text.ShouldEqual(items[1].ToString());
		}

		[Test]
		public void should_remove_standalone_paragraph_endings()
		{
			var items = new IItem[]
			{
				new ParagraphEnd(),
				new ParagraphEnd(),
				new Line {Text = "line after double paragraph end"}
			};

			var results = _cut.CollapseContentItems(items).ToArray();

			results.Length.ShouldEqual(1);
			results.First().ShouldBeOfType<Line>();
			var line = (Line)results.First();
			line.Text.ShouldEqual(items[2].ToString());
		}

		[Test]
		public void should_leave_lines_without_trailing_paragraph_end_alone()
		{
			var items = new IItem[]
			{
				new Line {Text = "line1"},
				new Line {Text = "line2"}
			};

			var results = _cut.CollapseContentItems(items).ToArray();

			results.Length.ShouldEqual(2);
			results[0].ShouldBeOfType<Line>();
			results[1].ShouldBeOfType<Line>();
		}

		[Test]
		public void should_collapse_items_which_have_nested_items()
		{
			var items = new IItem[]
			{
				new OriginalMessage { Header = "original header", Items = new IItem[]
				{
					new Line(),
					new Line(),
					new ParagraphEnd(),
					new Line()
				}}
			};

			var results = _cut.CollapseContentItems(items).ToArray();

			results.Length.ShouldEqual(1);
			results[0].ShouldBeOfType<OriginalMessage>();

			var originalMessage = (OriginalMessage) results[0];
			var originalMessageItems = originalMessage.Items.ToArray();

			originalMessageItems.Length.ShouldEqual(2);
			originalMessageItems[0].ShouldBeOfType<Paragraph>();
			originalMessageItems[1].ShouldBeOfType<Line>();
		}
	}
}
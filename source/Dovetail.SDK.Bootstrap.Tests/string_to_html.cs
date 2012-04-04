using System;
using Dovetail.SDK.Bootstrap.Extensions;
using NUnit.Framework;

namespace Dovetail.SDK.Bootstrap.Tests
{
    [TestFixture]
    public class string_to_html
    {
        [Test]
        public void newlinep()
        {
            Environment.NewLine.ShouldEqual("\r\n");
        }

        [Test]
        public void single_line_wrapped_in_a_p()
        {
            const string input = @"line1";

            var result = input.ToHtml();

            result.ShouldEqual("<p>line1</p>");
        }

        [Test]
        public void each_line_wrapped_in_a_p()
        {
            const string input = "line1\r\nline2";
            Console.WriteLine(input);
            var result = input.ToHtml();

            result.ShouldEqual("<p>line1</p>\n<p>line2</p>");
        }

        [Test]
        public void multiple_line_returns_are_collapsed()
        {
            const string input = @"line1


line2";

            var result = input.ToHtml();

            result.ShouldEqual("<p>line1</p>\n<p>line2</p>");
        }
    }
}
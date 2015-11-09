using System.Text.RegularExpressions;

namespace Dovetail.SDK.Bootstrap
{
	public interface IUrlLinkifier
	{
		string Linkify(string text);
	}

	public class UrlLinkifier : IUrlLinkifier
	{
		// Based on URL regex from John Gruber:
		// http://daringfireball.net/
		// Copied from Chad Myers' repo:
		// https://github.com/chadmyers/UrlRegex
		public static readonly Regex UrlExpression = new Regex(@"(?<link>\b((?:[a-z][\w-]+:(?:\/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}\/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’])))", RegexOptions.IgnoreCase);

		public string Linkify(string text)
		{
			return UrlExpression.Replace(text, @"<a href=""${link}"">${link}</a>");
		}
	}
}

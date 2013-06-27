using System.Text.RegularExpressions;

namespace Dovetail.SDK.Bootstrap
{
	public interface IUrlLinkifier
	{
		string Linkify(string text);
	}

	public class UrlLinkifier : IUrlLinkifier
	{
		private static readonly Regex urlFinderRegEx = new Regex(@"(?<link>(?<protocol>ftp|http|https|mailto|file|webcal):(?:(?:[A-Za-z0-9$_.+!*(),;/?:@&~=-])|%[A-Fa-f0-9]{2}){2,}(?:#(?:[a-zA-Z0-9][a-zA-Z0-9$_.+!*(),;/?:@&~=%-]*))?(?:[A-Za-z0-9$_+!*();/?:~-]))");

		public string Linkify(string text)
		{
			return urlFinderRegEx.Replace(text, @"<a href=""${link}"">${link}</a>");
		}
	}
}
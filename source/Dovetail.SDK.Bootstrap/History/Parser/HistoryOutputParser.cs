using System;
using System.Text.RegularExpressions;
using Dovetail.SDK.Bootstrap.Configuration;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.History.Parser
{
	public interface IHistoryOutputParser
	{
		string Encode(string input);
	}

	public class HistoryOutputParser : IHistoryOutputParser
	{
		private readonly IOutputEncoder _outputEncoder;

		public HistoryOutputParser(IOutputEncoder outputEncoder)
		{
			_outputEncoder = outputEncoder;
		}

		private static readonly Regex urlFinderRegEx = new Regex(@"(?<link>(?<protocol>ftp|http|https|mailto|file|webcal):(?:(?:[A-Za-z0-9$_.+!*(),;/?:@&~=-])|%[A-Fa-f0-9]{2}){2,}(?:#(?:[a-zA-Z0-9][a-zA-Z0-9$_.+!*(),;/?:@&~=%-]*))?(?:[A-Za-z0-9$_+!*();/?:~-]))");

		public string Encode(string input)
		{
			if (input.IsEmpty()) return String.Empty;

			var output = _outputEncoder.Encode(input);

			var items = new HistoryItemParser().Parse(output);
			var result = new HistoryItemHtmlRenderer().Render(items);
			result = urlFinderRegEx.Replace(result, @"<a href=""${link}"">${link}</a>");

			return result;
		}
	}
}
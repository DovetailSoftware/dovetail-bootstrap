using System.Text.RegularExpressions;

namespace Dovetail.SDK.Bootstrap.History.Parser
{
	public interface IParagraphEndLocator
	{
		string LocateAndReplace(string input);
	}

	public class ParagraphEndLocator : IParagraphEndLocator
	{
		public const string ENDOFPARAGRAPHTOKEN = "__EOP__";

		public string LocateAndReplace(string input)
		{
			return Regex.Replace(input, @"(\n|\r\n)\s*(\n|\r\n)+", "\n" + ENDOFPARAGRAPHTOKEN + "\n");
		}
	}
}
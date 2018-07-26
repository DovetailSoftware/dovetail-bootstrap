using System.Text.RegularExpressions;
using Dovetail.SDK.ModelMap.Transforms;

namespace Dovetail.SDK.History
{
	public class AdditionalInfoLexerTransform : IMappingTransform
	{
		public const string Key = "$additionalInfo";

		public object Execute(TransformContext context)
		{
			var regex = new Regex(context.Arguments.Get<string>("pattern"));
			var details = context.Model.Child("details");
			var additionalInfo = details.Get<string>(Key);
			var match = regex.Match(additionalInfo);

			if (match.Success)
			{
				var groups = regex.GetGroupNames();
				foreach (var group in groups)
				{
					int number;
					if (int.TryParse(group, out number)) continue;

					var value = match.Groups[group].Value;
					details[group] = value;
				}
			}

			details.Remove(Key);

			return context.Model;
		}
	}
}

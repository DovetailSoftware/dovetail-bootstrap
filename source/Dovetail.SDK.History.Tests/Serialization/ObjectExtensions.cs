using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dovetail.SDK.History.Tests.Serialization
{
	public static class ObjectExtensions
	{
		public static string ToJSON(this object o)
		{
			var settings = new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				DateTimeZoneHandling = DateTimeZoneHandling.Utc
			};

			var jsonSerializer = JsonSerializer.Create(settings);

			var sb = new StringBuilder(256);
			var sw = new StringWriter(sb, CultureInfo.InvariantCulture);
			using (var jsonWriter = new JsonTextWriter(sw))
			{
				jsonWriter.Formatting = Formatting.None;
				jsonWriter.StringEscapeHandling = StringEscapeHandling.EscapeHtml;

				jsonSerializer.Serialize(jsonWriter, o);
			}

			return sw.ToString();
		}
	}
}
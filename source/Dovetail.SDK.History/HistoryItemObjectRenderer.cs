using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.History.Parser;
using FubuCore;

namespace Dovetail.SDK.History
{
	public class HistoryItemObjectRenderer : IHistoryItemObjectRenderer
	{
		public IDictionary<string, object> Render(IItem item)
		{
			var data = new Dictionary<string, object>();
			Render(item, data);

			return data;
		}

		public IDictionary<string, object>[] Render(IEnumerable<IItem> items)
		{
			var data = new List<IDictionary<string, object>>();
			foreach (var item in items)
			{
				var output = new Dictionary<string, object>();
				Render(item, output);
				data.Add(output);
			}

			return data.ToArray();
		}

		public void Render(IItem item, Dictionary<string, object> output)
		{
			output["type"] = item.GetType().Name;

			if (item.GetType().CanBeCastTo<EmailLog>())
			{
				var emailLog = (EmailLog)item;
				output["header"] = emailLog.Header;
				output["items"] = Render(emailLog.Items);
				return;
			}

			if (item.GetType().CanBeCastTo<EmailHeader>())
			{
				output["header"] = item.As<EmailHeader>();
				return;
			}

			if (item.GetType().CanBeCastTo<BlockQuote>())
			{
				output["lines"] = item.As<BlockQuote>().Lines;
				return;
			}

			if (item.GetType().CanBeCastTo<OriginalMessage>())
			{
				var original = (OriginalMessage)item;
				output["header"] = original.Header;
				output["items"] = Render(original.Items);
				return;
			}

			item.GetType().GetProperties().Each(_ =>
			{
				output.Add(_.Name.Substring(0, 1).ToLower() + _.Name.Substring(1), _.GetValue(item, null));
			});
		}
	}
}
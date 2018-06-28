using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.History;
using Dovetail.SDK.Bootstrap.History.Parser;
using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Transforms;
using FubuCore;

namespace Dovetail.SDK.History
{
	public class EmailParserTransform : IMappingTransform
	{
		public object Execute(TransformContext context)
		{
			var input = context.Arguments.Get<string>("input");
			var data = new ModelData();
			context
				.Service<IEmailParser>()
				.Parse(formatMessage(input, context.Model))
				.Each(_ => data[_.Key] = _.Value);
			
			return data;
		}

		private string formatMessage(string message, ModelData data)
		{
			var log = new StringBuilder();

			var from = data.Get<string>("sender");
			var to = data.Get<string>("recipient");
			var cclist = "";
			if (data.Has("ccList"))
			{
				cclist = data.Get<string>("ccList");
				if (cclist != null) cclist = cclist.Trim();
			}

			var subject = data.Get<string>("subject") ?? "";
			var isoDate = data.Get<DateTime>("timestamp").ToString("s", CultureInfo.InvariantCulture);

			log.Append(HistoryParsers.BEGIN_EMAIL_LOG_HEADER);

			log.AppendLine("{0}: {1}{2}".ToFormat(HistoryBuilderTokens.LOG_EMAIL_DATE, HistoryParsers.BEGIN_ISODATE_HEADER, isoDate));
			const string headerFormat = "{0}: {1}";
			log.AppendLine(headerFormat.ToFormat(HistoryBuilderTokens.LOG_EMAIL_FROM, from));
			log.AppendLine(headerFormat.ToFormat(HistoryBuilderTokens.LOG_EMAIL_TO, to));
			if (cclist.IsNotEmpty()) log.AppendLine(headerFormat.ToFormat(HistoryBuilderTokens.LOG_EMAIL_CC, cclist));
			if (subject.IsNotEmpty()) log.AppendLine(headerFormat.ToFormat(HistoryBuilderTokens.LOG_EMAIL_SUBJECT, subject));

			log.Append(HistoryParsers.END_EMAIL_LOG_HEADER);

			log.AppendLine(message);

			return log.ToString();
		}
	}
}
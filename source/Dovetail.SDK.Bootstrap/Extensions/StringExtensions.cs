using System;
using System.Text;
using System.Web;

namespace Dovetail.SDK.Bootstrap.Extensions
{
	public static class StringExtensions
	{
		public static string HtmlEncode(this string encodeMe)
		{
			return HttpUtility.HtmlEncode(encodeMe);
		}

        private static readonly Random _random = new Random(((int)DateTime.Now.Ticks));

        public static string GetRandomString(int size)
        {
            var builder = new StringBuilder(size + 10);
            for (var i = 0; i < size; i++)
            {
                var character = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * _random.NextDouble() + 65)));
                builder.Append(character);
            }
            return builder.ToString().ToLower();
        }
	}
}
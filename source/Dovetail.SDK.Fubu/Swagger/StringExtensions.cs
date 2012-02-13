namespace Dovetail.SDK.Fubu.Swagger
{
    public static class StringExtensions
    {
        public static string UrlRelativeTo(this string baseUrl, string targetUrl)
        {
            var b = baseUrl;
            var t = targetUrl;

            var lastSlashIndex = b.LastIndexOf('/');
            if (lastSlashIndex == -1) return t;
            
            b = b.Substring(0, lastSlashIndex + 1);

            if (!b.StartsWith("/")) b = "/" + b;
            if (!t.StartsWith("/")) t = "/" + t;

            if (t.StartsWith(b))
                t = t.Remove(0, b.Length);

            if (!t.StartsWith("/")) t = "/" + t;

            return t;
        }
    }
}
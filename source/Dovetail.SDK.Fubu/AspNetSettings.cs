using System.Web;

namespace Dovetail.SDK.Fubu
{
    public static class AspNetSettings
    {
        private static bool? _isCustomErrorsEnabled;

        public static bool IsCustomErrorsEnabled
        {
            get
            {
                return _isCustomErrorsEnabled ?? HttpContext.Current != null && HttpContext.Current.IsCustomErrorEnabled;
            }

            set { _isCustomErrorsEnabled = value; }
        }
    }
}
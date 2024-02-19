using FChoice.Foundation.Clarify;
using FChoice.Foundation.DataObjects;
using FChoice.Foundation.Schema;

namespace Dovetail.SDK.Clarify
{
    public interface IClarifyContext
    {
        ClarifyApplication Application { get; }
        ISchemaCache SchemaCache { get; }
        ILocaleCache LocaleCache { get; }
        ITimeZone ServerTimeZone { get; }

        IClarifySession CreateSession();
		  IClarifySession CreateSession(string userName, ClarifyLoginType loginType);
    }
}
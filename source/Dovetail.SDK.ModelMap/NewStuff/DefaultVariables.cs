using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Clarify;
using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff
{
    public class DefaultVariables : IMappingVariableSource
    {
        public IEnumerable<IMappingVariable> Variables()
        {
            yield return new SdkUserIdVariable();
			yield return new SdkUserNameVariable();
		}
    }

    public class SdkUserIdVariable : MappingVariable
    {
        public override string Key { get { return "sessionUserId"; } }

        public override object Expand(string key, IServiceLocator services)
        {
            return services.GetInstance<IClarifySession>().SessionUserID;
        }
    }

	public class SdkUserNameVariable : MappingVariable
	{
		public override string Key { get { return "sessionUserName"; } }

		public override object Expand(string key, IServiceLocator services)
		{
			return services.GetInstance<IClarifySession>().UserName;
		}
	}
}
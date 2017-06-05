using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Clarify;

namespace Dovetail.SDK.ModelMap
{
    public class DefaultVariables : IMappingVariableSource
    {
        public IEnumerable<IMappingVariable> Variables()
        {
            yield return new SdkUserIdVariable();
			yield return new SdkUserNameVariable();
	        yield return new ModelDataVariable();
        }
    }

    public class SdkUserIdVariable : MappingVariable
    {
        public override string Key { get { return "sessionUserId"; } }

        public override object Expand(VariableExpansionContext context)
        {
            return context.Service<IClarifySession>().SessionUserID;
        }
    }

	public class SdkUserNameVariable : MappingVariable
	{
		public override string Key { get { return "sessionUserName"; } }

		public override object Expand(VariableExpansionContext context)
		{
			return context.Service<IClarifySession>().UserName;
		}
	}

	public class ModelDataVariable : IMappingVariable
	{
		public bool Matches(VariableExpansionContext context)
		{
			return context.Data != null && context.Key.StartsWith("this.");
		}

		public object Expand(VariableExpansionContext context)
		{
			var key = context.Key.Substring("this.".Length);
			return context.Data[key];
		}
	}
}
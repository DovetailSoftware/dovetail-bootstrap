using System.Collections.Generic;

namespace Dovetail.SDK.History.Serialization
{
	public interface IHistoryPrivilegePolicyCache
	{
		IEnumerable<PrivilegePolicy> Find(string objectType, int actCode);
		IEnumerable<PrivilegePolicy> GetAll();
	}
}

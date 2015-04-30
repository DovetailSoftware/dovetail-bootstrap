using System.Web;
using Dovetail.SDK.Bootstrap.Authentication;
using StructureMap;

namespace Dovetail.SDK.Bootstrap.Configuration
{
	public class SecurityContextProvider
	{
		private readonly IContainer _container;

		public SecurityContextProvider(IContainer container)
		{
			_container = container;
		}

		public ISecurityContext GetSecurityContext()
		{
			if (HttpContext.Current == null)
			{
				return _container.GetInstance<NullSecurityContext>();
			}

			return _container.GetInstance<AspNetSecurityContext>();
		}
	}
}
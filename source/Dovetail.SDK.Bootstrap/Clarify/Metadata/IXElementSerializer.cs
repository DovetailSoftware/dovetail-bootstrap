using System.Xml.Linq;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public interface IXElementSerializer
	{
		object ValueFor(XAttribute attribute);
		T Deserialize<T>(XElement element) where T : class, new();
	}
}

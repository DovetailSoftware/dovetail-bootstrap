using System.Xml.Linq;

namespace Dovetail.SDK.ModelMap.Serialization
{
    public interface IXElementService
    {
        void Visit(XElement element, ModelMap map, ParsingContext context);
    }
}
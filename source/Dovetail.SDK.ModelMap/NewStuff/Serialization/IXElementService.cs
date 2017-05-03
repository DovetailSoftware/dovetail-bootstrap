using System.Xml.Linq;

namespace Dovetail.SDK.ModelMap.NewStuff.Serialization
{
    public interface IXElementService
    {
        void Visit(XElement element, ModelMap map, ParsingContext context);
    }
}
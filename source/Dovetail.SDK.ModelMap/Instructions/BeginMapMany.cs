using System;
using System.Reflection;

namespace Dovetail.SDK.ModelMap.Instructions
{
    public class BeginMapMany
    {
        public Type ParentModelType { get; set; }
        public Type ChildModelType { get; set; }
        public PropertyInfo MappedProperty { get; set; }
    }
}
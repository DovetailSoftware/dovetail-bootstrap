using System;
using Dovetail.SDK.ModelMap.Clarify;
using Dovetail.SDK.ModelMap.Legacy.Registration.DSL;
using FubuCore;

namespace Dovetail.SDK.ModelMap.ObjectModel
{
    public class FieldMap
    {
        public string Key { get; set; }
        public string ModelName { get; set; }
        public Type PropertyType { get; set; }

        public bool ShouldEncode { get; set; }
        public bool IsIdentifier { get; set; }
        public string[] FieldNames { get; set; }
        
        public GlobalListConfigurationExpression GlobalListConfig { get; set; }
        public UserDefinedList UserDefinedList { get; set; }
        public Func<object> FieldValueMethod { get; set; }
        public Func<string, object> StringToFieldValueMethod { get; set; } //TODO can this be combined with MapFieldValuesToObject?? - kjm
        public Func<string[], object> MapFieldValuesToObject { get; set; }

        public FieldMap()
        {
            ShouldEncode = true;
        }

        public object GetFieldValue()
        {
            return FieldValueMethod();
        }

        public override string ToString()
        {
            var fields = String.Join(", ", FieldNames);
            return "map field(s) {0} to property {1} on model {2}.".ToFormat(fields, Key, ModelName);
        }
    }
}
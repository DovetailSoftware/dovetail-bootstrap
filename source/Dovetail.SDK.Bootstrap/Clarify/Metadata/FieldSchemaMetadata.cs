using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public class FieldSchemaMetadata : SchemaMetadata
	{
		public string Name { get; set; }
		public string DataType { get; set; }

		public bool IsDateField()
		{
			return "date".EqualsIgnoreCase(DataType);
		}
	}
}

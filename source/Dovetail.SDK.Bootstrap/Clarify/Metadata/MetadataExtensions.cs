using FChoice.Foundation.Clarify.Schema;
using FChoice.Foundation.Schema;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public static class MetadataExtensions
	{
		public static FieldSchemaMetadata MetadataFor(this ISchemaMetadataCache cache, ISchemaField field)
		{
			var clarifyField = field as SchemaFieldBase;
			if (clarifyField == null)
				return new FieldSchemaMetadata { Name = field.Name };

			var tableName = clarifyField.Parent.Name;
			var tableMetadata = cache.MetadataFor(tableName);

			return tableMetadata.MetadataFor(field.Name);
		}
	}
}

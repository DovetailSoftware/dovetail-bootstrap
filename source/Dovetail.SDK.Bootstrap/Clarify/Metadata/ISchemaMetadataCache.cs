namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public interface ISchemaMetadataCache
	{
		TableSchemaMetadata MetadataFor(string tableName);
	}
}

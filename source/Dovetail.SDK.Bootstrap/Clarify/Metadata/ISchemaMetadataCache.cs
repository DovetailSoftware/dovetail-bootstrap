using System.Collections.Generic;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public interface ISchemaMetadataCache
	{
		IEnumerable<TableSchemaMetadata> Tables { get; }
		TableSchemaMetadata MetadataFor(string tableName);
	}
}

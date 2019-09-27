using System.Collections.Generic;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public interface ISchemaMetadata
	{
		void Add(ISchemaMetadatum metadatum);
		TMetadatum Datum<TMetadatum>() where TMetadatum : ISchemaMetadatum;
		IEnumerable<TMetadatum> Data<TMetadatum>() where TMetadatum : ISchemaMetadatum;
	}
}

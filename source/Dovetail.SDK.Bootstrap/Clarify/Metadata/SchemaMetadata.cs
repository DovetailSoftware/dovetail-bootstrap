using System.Collections.Generic;
using System.Linq;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public abstract class SchemaMetadata : ISchemaMetadata
	{
		private readonly IList<ISchemaMetadatum> _metadata = new List<ISchemaMetadatum>();

		public void Add(ISchemaMetadatum metadatum)
		{
			_metadata.Add(metadatum);
		}

		public TMetadatum Datum<TMetadatum>() where TMetadatum : ISchemaMetadatum
		{
			return Data<TMetadatum>().SingleOrDefault();
		}

		public IEnumerable<TMetadatum> Data<TMetadatum>() where TMetadatum : ISchemaMetadatum
		{
			return _metadata.OfType<TMetadatum>();
		}
	}
}

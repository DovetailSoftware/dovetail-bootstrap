using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public class TableSchemaMetadata : SchemaMetadata
	{
		private readonly IList<FieldSchemaMetadata> _fields = new List<FieldSchemaMetadata>();

		public string Name { get; set; }

		public IEnumerable<FieldSchemaMetadata> Fields
		{
			get { return _fields; }
		}

		public void AddField(FieldSchemaMetadata field)
		{
			_fields.Add(field);
		}

		public FieldSchemaMetadata MetadataFor(string fieldName)
		{
			var field = _fields.SingleOrDefault(_ => _.Name.EqualsIgnoreCase(fieldName));
			if (field == null)
			{
				field = new FieldSchemaMetadata { Name = fieldName };
				_fields.Add(field);
			}

			return field;
		}
	}
}

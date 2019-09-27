using System;

namespace Dovetail.SDK.Bootstrap.Clarify.Metadata
{
	public class SchemaMetadataSettings
	{
		public const string FileName = "schema.metadata.config";

		public SchemaMetadataSettings()
		{
			Path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
		}

		public string Path { get; set; }
	}
}
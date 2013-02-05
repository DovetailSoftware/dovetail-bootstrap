namespace Dovetail.SDK.Bootstrap.Configuration
{
	public class DoNothingOutputEncoder : IOutputEncoder
	{
		public string Encode(string encodeMe)
		{
			return encodeMe;
		}
	}
}
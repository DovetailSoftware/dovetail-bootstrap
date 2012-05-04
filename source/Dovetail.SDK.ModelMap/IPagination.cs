namespace Dovetail.SDK.ModelMap
{
	public interface IPagination 
	{
		int PageSize { get; set; }
		int CurrentPage { get; set; }
		int TotalCount { get; set; }
	}

	public class Pagination : IPagination
	{
		public int PageSize { get; set; }
		public int CurrentPage { get; set; }
		public int TotalCount { get; set; }
	}
}
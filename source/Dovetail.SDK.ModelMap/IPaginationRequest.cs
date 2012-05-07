namespace Dovetail.SDK.ModelMap
{
	public interface IPaginationRequest 
	{
		int PageSize { get; set; }
		int CurrentPage { get; set; }
	}

	public class PaginationRequest : IPaginationRequest
	{
		public int PageSize { get; set; }
		public int CurrentPage { get; set; }
	}
}
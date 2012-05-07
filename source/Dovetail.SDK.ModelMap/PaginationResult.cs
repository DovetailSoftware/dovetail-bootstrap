namespace Dovetail.SDK.ModelMap
{
	public interface IPaginationResult<MODEL>
	{
		MODEL[] Models { get; set; }
		int PageSize { get; set; }
		int CurrentPage { get; set; }
		int TotalRecordCount { get; set; }
	}

	public class PaginationResult<MODEL> : IPaginationResult<MODEL>
	{
		public MODEL[] Models { get; set; }
		public int PageSize { get; set; }
		public int CurrentPage { get; set; }
		public int TotalRecordCount { get; set; }
	}
}
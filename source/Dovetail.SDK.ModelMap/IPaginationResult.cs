namespace Dovetail.SDK.ModelMap
{
    public interface IPaginationResult
    {
        ModelData[] Models { get; set; }
        int PageSize { get; set; }
        int CurrentPage { get; set; }
        int TotalRecordCount { get; set; }
    }
}
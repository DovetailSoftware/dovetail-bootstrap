namespace Dovetail.SDK.ModelMap.NewStuff
{
    public interface IPaginationResult
    {
        ModelData[] Models { get; set; }
        int PageSize { get; set; }
        int CurrentPage { get; set; }
        int TotalRecordCount { get; set; }
    }
}
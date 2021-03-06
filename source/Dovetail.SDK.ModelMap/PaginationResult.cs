﻿namespace Dovetail.SDK.ModelMap
{
    public class PaginationResult : IPaginationResult
    {
        public ModelData[] Models { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalRecordCount { get; set; }
    }
}
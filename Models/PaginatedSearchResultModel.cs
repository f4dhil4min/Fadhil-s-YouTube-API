using System.Collections.Generic;

namespace ProjectYoutube.Models
{
    public class PaginatedSearchResultModel
    {
        public List<YouTubeVideoModel> Videos { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string Query { get; set; }
        public string SortBy { get; set; }
        public string Duration { get; set; }
        public string Category { get; set; }
        public string UploadDate { get; set; }
    }
}

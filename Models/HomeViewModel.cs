using System.Collections.Generic;

namespace ProjectYoutube.Models
{
    public class HomeViewModel
    {
        public List<YouTubeVideoModel> TrendingVideos { get; set; }
        public List<string> RecentSearches { get; set; }
    }
}

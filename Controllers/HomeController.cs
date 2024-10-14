using Microsoft.AspNetCore.Mvc;
using ProjectYoutube.Models;
using ProjectYoutube.Services; // Assuming you have YouTubeApiService for fetching trending videos
using System.Diagnostics;

namespace ProjectYoutube.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly YouTubeApiService _youtubeService;

        public HomeController(ILogger<HomeController> logger, YouTubeApiService youtubeService)
        {
            _logger = logger;
            _youtubeService = youtubeService;
        }

        // Home page with new features like Trending Videos and Recent Searches
        public async Task<IActionResult> Index()
        {
            var trendingVideos = await _youtubeService.GetTrendingVideosAsync(); // Fetch trending videos
            var model = new HomeViewModel // Assuming you create a new HomeViewModel to hold data
            {
                TrendingVideos = trendingVideos,
                RecentSearches = GetRecentSearches() // Method to retrieve recent searches from session/local storage
            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Helper method to get recent searches (store in session or local storage)
        private List<string> GetRecentSearches()
        {
            var recentSearches = HttpContext.Session.GetString("RecentSearches"); // Assuming you're using Session
            if (!string.IsNullOrEmpty(recentSearches))
            {
                return recentSearches.Split(',').ToList();
            }
            return new List<string>(); // Return empty list if no searches found
        }
    }
}

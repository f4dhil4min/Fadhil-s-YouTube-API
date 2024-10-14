using Microsoft.AspNetCore.Mvc;
using ProjectYoutube.Services;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using ProjectYoutube.Models;

namespace ProjectYoutube.Controllers
{
    public class YouTubeController : Controller
    {
        private readonly YouTubeApiService _youtubeService;
        private const int VideosPerPage = 6; // Number of videos per page

        public YouTubeController(YouTubeApiService youtubeService)
        {
            _youtubeService = youtubeService;
        }

        // Render the search form
        [HttpGet]
        public IActionResult Search()
        {
            return View(); // Loads Search.cshtml
        }

        // Handle the search query and redirect to SearchResult
        [HttpPost]
        public async Task<IActionResult> Search(string query, string sortBy, string duration, string category, string uploadDate)
        {
            return RedirectToAction("SearchResult", new { query, sortBy, duration, category, uploadDate }); // Redirect with filter params
        }

        // Render the SearchResult page with paginated search results
        [HttpGet]
        public async Task<IActionResult> SearchResult(string query, string sortBy, string duration, string category, string uploadDate, int page = 1)
        {
            // Fetch all videos with filters
            var videos = await _youtubeService.SearchVideosWithFiltersAsync(query, uploadDate, duration, category, sortBy);

            // Pagination logic
            int totalVideos = videos.Count();
            int totalPages = (int)System.Math.Ceiling((double)totalVideos / VideosPerPage);
            var paginatedVideos = videos.Skip((page - 1) * VideosPerPage).Take(VideosPerPage).ToList();

            // Create a model for pagination info
            var model = new PaginatedSearchResultModel
            {
                Videos = paginatedVideos,
                CurrentPage = page,
                TotalPages = totalPages,
                Query = query,
                SortBy = sortBy,
                Duration = duration,
                Category = category,
                UploadDate = uploadDate
            };

            return View(model); // Loads SearchResult.cshtml with paginated search results
        }

        // New Action for Search Suggestions (Autocomplete)
        [HttpGet]
        public async Task<IActionResult> GetSearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { suggestions = new string[0] }); // Return empty if no input
            }

            // Fetch the videos based on the search query
            var videos = await _youtubeService.SearchVideosAsync(query, maxResults: 5); // Limit to 5 suggestions

            // Extract and return only video titles and thumbnails as suggestions
            var suggestions = videos.Select(v => new
            {
                title = v.Title,
                thumbnail = v.ThumbnailUrl
            }).ToArray();

            return Json(new { suggestions }); // Return as JSON for autocomplete
        }

        // New Action for Trending Searches
        [HttpGet]
        public async Task<IActionResult> GetTrendingSearches()
        {
            // Fetch trending searches, adjust based on your implementation
            var trendingSearches = await _youtubeService.GetTrendingVideosAsync(maxResults: 5); // Use your trending method
            var trendingTitles = trendingSearches.Select(v => v.Title).ToArray();
            return Json(new { trending = trendingTitles }); // Return trending titles as JSON
        }
    }
}

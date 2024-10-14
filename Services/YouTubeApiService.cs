using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using ProjectYoutube.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Configuration;

namespace ProjectYoutube.Services
{
    public class YouTubeApiService
    {
        private readonly string _apiKey;

        public YouTubeApiService(IConfiguration configuration)
        {
            _apiKey = configuration["YouTubeApiKey"];
        }

        // Search videos without filters
        public async Task<List<YouTubeVideoModel>> SearchVideosAsync(string query, int maxResults = 50)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _apiKey,
                ApplicationName = "Fadhil's YouTube API"
            });

            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = query;
            searchRequest.MaxResults = maxResults;
            searchRequest.Type = "video"; // Ensure only video results are fetched

            var searchResponse = await searchRequest.ExecuteAsync();

            var videoIds = searchResponse.Items.Select(item => item.Id.VideoId).ToList();

            // Fetch video details to get additional information like duration
            var videosListRequest = youtubeService.Videos.List("snippet,contentDetails");
            videosListRequest.Id = string.Join(",", videoIds);

            var videoDetailsResponse = await videosListRequest.ExecuteAsync();

            var videos = videoDetailsResponse.Items.Select(item => new YouTubeVideoModel
            {
                Title = item.Snippet.Title,
                Description = item.Snippet.Description,
                ThumbnailUrl = item.Snippet.Thumbnails.Medium.Url, // Use Medium thumbnail
                VideoId = item.Id,
                Duration = ParseDuration(item.ContentDetails.Duration),
                Channel = item.Snippet.ChannelTitle,
                ChannelThumbnailUrl = item.Snippet.Thumbnails.High.Url // Use High thumbnail for channel
            }).ToList();


            return videos;
        }

        // Search videos with filters
        public async Task<List<YouTubeVideoModel>> SearchVideosWithFiltersAsync(string query, string uploadDate,
        string videoDuration, string category,string sortBy, int maxResults = 50)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _apiKey,
                ApplicationName = "Fadhil's YouTube API"
            });

            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = query;
            searchRequest.MaxResults = maxResults;
            searchRequest.Type = "video"; // Ensure only video results are fetched

            // Apply filters
            if (!string.IsNullOrEmpty(uploadDate))
            {
                switch (uploadDate)
                {
                    case "today":
                        searchRequest.PublishedAfter = DateTime.UtcNow.AddDays(-1);
                        break;
                    case "thisWeek":
                        searchRequest.PublishedAfter = DateTime.UtcNow.AddDays(-7);
                        break;
                    case "thisMonth":
                        searchRequest.PublishedAfter = DateTime.UtcNow.AddMonths(-1);
                        break;
                    case "thisYear":
                        searchRequest.PublishedAfter = DateTime.UtcNow.AddYears(-1);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(videoDuration))
            {
                // Use Any if Short and Long are not defined
                searchRequest.VideoDuration = videoDuration switch
                {
                    "short" => Google.Apis.YouTube.v3.SearchResource.ListRequest.VideoDurationEnum.Any,
                    "medium" => Google.Apis.YouTube.v3.SearchResource.ListRequest.VideoDurationEnum.Any,
                    "long" => Google.Apis.YouTube.v3.SearchResource.ListRequest.VideoDurationEnum.Any,
                    _ => Google.Apis.YouTube.v3.SearchResource.ListRequest.VideoDurationEnum.Any
                };
            }

            if (!string.IsNullOrEmpty(category))
            {
                searchRequest.VideoCategoryId = category; // Use category ID
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                searchRequest.Order = sortBy switch
                {
                    "relevance" => Google.Apis.YouTube.v3.SearchResource.ListRequest.OrderEnum.Relevance,
                    "date" => Google.Apis.YouTube.v3.SearchResource.ListRequest.OrderEnum.Date,
                    "viewCount" => Google.Apis.YouTube.v3.SearchResource.ListRequest.OrderEnum.ViewCount,
                    "rating" => Google.Apis.YouTube.v3.SearchResource.ListRequest.OrderEnum.Rating,
                    _ => Google.Apis.YouTube.v3.SearchResource.ListRequest.OrderEnum.Relevance
                };
            }

            var searchResponse = await searchRequest.ExecuteAsync();

            var videoIds = searchResponse.Items.Select(item => item.Id.VideoId).ToList();

            // Fetch video details to get additional information like duration
            var videosListRequest = youtubeService.Videos.List("snippet,contentDetails");
            videosListRequest.Id = string.Join(",", videoIds);

            var videoDetailsResponse = await videosListRequest.ExecuteAsync();

            var videos = videoDetailsResponse.Items.Select(item => new YouTubeVideoModel
            {
                Title = item.Snippet.Title,
                Description = item.Snippet.Description,
                ThumbnailUrl = item.Snippet.Thumbnails.Medium.Url,
                VideoId = item.Id,
                Duration = ParseDuration(item.ContentDetails.Duration), // Format duration
                Channel = item.Snippet.ChannelTitle,
                ChannelThumbnailUrl = item.Snippet.Thumbnails.High.Url // Use High thumbnail for channel
            }).ToList();

            return videos;
        }

        // Get Trending Videos
        public async Task<List<YouTubeVideoModel>> GetTrendingVideosAsync(int maxResults = 10)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _apiKey,
                ApplicationName = "Fadhil's YouTube API"
            });

            var videosListRequest = youtubeService.Videos.List("snippet,contentDetails");
            videosListRequest.Chart = Google.Apis.YouTube.v3.VideosResource.ListRequest.ChartEnum.MostPopular;
            videosListRequest.MaxResults = maxResults;

            var response = await videosListRequest.ExecuteAsync();

            var videos = response.Items.Select(item => new YouTubeVideoModel
            {
                Title = item.Snippet.Title,
                Description = item.Snippet.Description,
                ThumbnailUrl = item.Snippet.Thumbnails.Medium.Url,
                VideoId = item.Id,
                Duration = ParseDuration(item.ContentDetails.Duration), // Format duration
                Channel = item.Snippet.ChannelTitle,
                ChannelThumbnailUrl = item.Snippet.Thumbnails.High.Url // Use High thumbnail for channel
            }).ToList();

            return videos;
        }

        // Helper method to parse the ISO 8601 duration format (PT#H#M#S)
        private string ParseDuration(string isoDuration)
        {
            var duration = XmlConvert.ToTimeSpan(isoDuration);
            return $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}"; // Format as HH:MM:SS
        }
    }
}

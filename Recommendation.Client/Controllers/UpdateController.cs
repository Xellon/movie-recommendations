using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Recommendation.Database;

namespace Recommendation.Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateController : ControllerBase
    {
        readonly HttpClient _client = new HttpClient();
        readonly IConfiguration _configuration;
        readonly TMDB.RequestBuilder _requestBuilder;
        private DatabaseContext _context;

        public UpdateController(IConfiguration configuration, DatabaseContext context)
        {
            _context = context;
            _configuration = configuration;
            _requestBuilder = new TMDB.RequestBuilder(configuration);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Everything()
        {
            StatusCodeResult result;

            result = await Tags();
            if (result.StatusCode != StatusCodes.Status200OK)
                return BadRequest();

            result = await Movies();
            if (result.StatusCode != StatusCodes.Status200OK)
                return BadRequest();

            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<StatusCodeResult> Movies()
        {
            var discoveredMovies = await RequestDiscoveredMovies(1);

            SaveMovies(discoveredMovies.Results);

            var totalPages = discoveredMovies.Total_Pages;

            for (int page = 2; page <= totalPages; page++)
            {
                discoveredMovies = await RequestDiscoveredMovies(page);
                SaveMovies(discoveredMovies.Results);

                if (page % 40 == 0)
                    await Task.Delay(10000);
            }

            return Ok();
        }

        private async Task<TMDB.DiscoveredMovies> RequestDiscoveredMovies(int page)
        {
            var response = await _client.GetAsync(
                _requestBuilder.CreateUri(
                    "discover/movie",
                    $"sort_by=popularity.desc&include_adult=false&include_video=false&page={page}&release_date.gte=1980&vote_average.gte=5"));

            return await response.Content.ReadAsAsync<TMDB.DiscoveredMovies>();
        }

        private async void SaveMovies(IEnumerable<TMDB.DiscoveredMovie> discoveredMovies)
        {
            foreach (var discoveredMovie in discoveredMovies)
            {
                var imageUrl = discoveredMovie.Poster_Path is null
                    ? ""
                    : $"https://image.tmdb.org/t/p/w200{discoveredMovie.Poster_Path}";

                var movie = new Movie()
                {
                    Id = discoveredMovie.Id,
                    AverageRating = discoveredMovie.Vote_Average,
                    Description = discoveredMovie.Overview,
                    ImageUrl = imageUrl,
                    Title = discoveredMovie.Title
                };

                _context.Movies.Attach(movie);
                await _context.Movies.AddAsync(movie);

                foreach (var tagId in discoveredMovie.Genre_Ids)
                {
                    var movieTag = new MovieTag()
                    {
                        MovieId = discoveredMovie.Id,
                        TagId = tagId
                    };

                    try
                    {
                        _context.MovieTags.Attach(movieTag);
                        await _context.MovieTags.AddAsync(movieTag);
                    }
                    catch { }
                }
                await _context.SaveChangesAsync();
            }
        }

        [HttpPost("[action]")]
        public async Task<StatusCodeResult> Tags()
        {
            var response = await _client.GetAsync(_requestBuilder.CreateUri("genre/movie/list"));

            var genresObject = await response.Content.ReadAsAsync<TMDB.GenresObject>();

            foreach (var genre in genresObject.Genres)
            {
                var tag = new Tag() { Id = genre.Id, Text = genre.Name };

                _context.Tags.Attach(tag);
                await _context.Tags.AddAsync(tag);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    namespace TMDB
    {
        public struct DiscoveredMovies
        {
            public int Page { get; set; }
            public int Total_Results { get; set; }
            public int Total_Pages { get; set; }
            public IEnumerable<DiscoveredMovie> Results { get; set; }
        }

        public struct DiscoveredMovie
        {
            public string Poster_Path { get; set; }
            public bool Adult { get; set; }
            public string Overview { get; set; }
            public DateTime Release_Date { get; set; }
            public IEnumerable<int> Genre_Ids { get; set; }
            public int Id { get; set; }
            public string Original_Title { get; set; }
            public string Original_Language { get; set; }
            public string Title { get; set; }
            public string Backdrop_Path { get; set; }
            public double Popularity { get; set; }
            public int Vote_Count { get; set; }
            public bool Video { get; set; }
            public double Vote_Average { get; set; }
        }

        public struct GenresObject
        {
            public IEnumerable<Genre> Genres { get; set; }
        }

        public struct Genre
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class RequestBuilder
        {
            readonly IConfiguration _configuration;

            public RequestBuilder(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public Uri CreateUri(string route)
            {
                return CreateUri(route, "");
            }

            public Uri CreateUri(string route, string queryParams)
            {
                var uriBase = _configuration.GetValue<string>("TMDB:Url");
                var apiKey = _configuration.GetValue<string>("TMDB:ApiKey");

                var path = Path.Join(uriBase, route);
                return new Uri($"{path}?api_key={apiKey}&language=en-US{(queryParams != "" ? "&" + queryParams : "")}");
            }
        }

    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Recommendation.Database;
using System.Runtime.Serialization;
using System.Linq;

namespace Recommendation.Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateController : ControllerBase
    {
        readonly HttpClient _client;
        readonly IConfiguration _configuration;
        readonly TMDB.RequestBuilder _requestBuilder;
        private DatabaseContext _context;

        public UpdateController(IConfiguration configuration, DatabaseContext context, HttpClient httpClient)
        {
            _context = context;
            _configuration = configuration;
            _client = httpClient;
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

            await SaveMovies(discoveredMovies.Results);

            var totalPages = discoveredMovies.Total_Pages;

            for (int page = 2; page <= totalPages; page++)
            {
                discoveredMovies = await RequestDiscoveredMovies(page);
                await SaveMovies(discoveredMovies.Results);

                if (page % 40 == 0)
                    await Task.Delay(10000);
            }

            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<StatusCodeResult> MovieCreators()
        {
            var movieIds = (
                from m in _context.Movies
                join mc in _context.MovieCreators on m.Id equals mc.MovieId into mcs // LEFT JOIN
                from mc in mcs.DefaultIfEmpty()
                where mc.CreatorId == null
                select m.Id
                );

            {
                int i = 0;
                foreach (var id in movieIds)
                {
                    var movieDetails = await RequestMovieDetails(id);

                    foreach (var company in movieDetails.Production_Companies)
                    {
                        var creator = new Creator
                        {
                            Id = company.Id,
                            Name = company.Name
                        };

                        if (!_context.Creators.Contains(creator))
                        {
                            _context.Creators.Attach(creator);
                            _context.Creators.Add(creator);
                            await _context.SaveChangesAsync();
                        }

                        var movieCreator = new MovieCreator
                        {
                            CreatorId = creator.Id,
                            MovieId = id
                        };

                        _context.MovieCreators.Attach(movieCreator);
                        _context.MovieCreators.Add(movieCreator);
                    }

                    await _context.SaveChangesAsync();

                    if (i % 40 == 0)
                        await Task.Delay(10000);
                    i++;
                }
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

        private async Task<TMDB.MovieDetails> RequestMovieDetails(int movieId)
        {
            var response = await _client.GetAsync(_requestBuilder.CreateUri($"movie/{movieId}"));

            return await response.Content.ReadAsAsync<TMDB.MovieDetails>();
        }

        private async Task SaveMovies(IEnumerable<TMDB.DiscoveredMovie> discoveredMovies)
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
                    Title = discoveredMovie.Title,
                    Date = discoveredMovie.Release_Date
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

        public enum MovieStatus
        {
            [EnumMember(Value = "Rumored")]
            Rumored,

            [EnumMember(Value = "Planned")]
            Planned,

            [EnumMember(Value = "In Production")]
            InProduction,

            [EnumMember(Value = "Post Production")]
            PostProduction,

            [EnumMember(Value = "Released")]
            Released,

            [EnumMember(Value = "Canceled")]
            Canceled,
        }

        public struct ProductionCompany
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public string Logo_path { get; set; }
            public string Origin_country { get; set; }
        }

        public struct ProductionCountry
        {
            public string Iso_3166_1 { get; set; }
            public string Name { get; set; }
        }

        public struct SpokenLanguage
        {
            public string Iso_639_1 { get; set; }
            public string Name { get; set; }
        }

        public struct MovieDetails
        {
            public bool Adult { get; set; }
            public string Backdrop_Path { get; set; }
            public int Budget { get; set; }
            public IEnumerable<Genre> Genres { get; set; }
            public string Homepage { get; set; }
            public int Id { get; set; }
            public string Imdb_Id { get; set; }
            public string Original_Language { get; set; }
            public string Original_Title { get; set; }
            public string Overview { get; set; }
            public double Popularity { get; set; }
            public string Poster_Path { get; set; }
            public IEnumerable<ProductionCompany> Production_Companies { get; set; }
            public IEnumerable<ProductionCountry> Production_Countries { get; set; }
            public DateTime Release_Date { get; set; }
            public int Revenue { get; set; }
            public int? Runtime { get; set; }
            public IEnumerable<SpokenLanguage> Spoken_Languages { get; set; }
            public MovieStatus Status { get; set; }
            public string Tagline { get; set; }
            public string Title { get; set; }
            public bool Video { get; set; }
            public double Vote_Average { get; set; }
            public int Vote_Count { get; set; }
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

            public Uri CreateUri(string route, string queryParams = "")
            {
                var uriBase = _configuration.GetValue<string>("TMDB:Url");
                var apiKey = _configuration.GetValue<string>("TMDB:ApiKey");

                var path = Path.Join(uriBase, route);
                return new Uri($"{path}?api_key={apiKey}&language=en-US{(queryParams != "" ? "&" + queryParams : "")}");
            }
        }

    }
}
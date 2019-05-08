using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Python.Runtime;
using System;
using System.IO;

namespace Recommendation.Service
{
    public class PythonEngineOptions
    {
        public string RecommendationCacheLocation { get; set; }
        public string SimilarityMatrixFilename { get; set; } = "similarityMatrix.txt";
        public string IdArrayFilename { get; set; } = "idArray.txt";
    }

    public class PythonRecommendationEngine : IRecommendationEngine
    {
        struct UserMovie
        {
            public int Id { get; set; }
            public float Rating { get; set; }
        }

        struct SimilarityObject
        {
            public int Index { get; set; }
            public double Similarity { get; set; }
        }
        class SimilarityObjectComparer : IEqualityComparer<SimilarityObject>
        {
            public bool Equals(SimilarityObject x, SimilarityObject y) => x.Index == y.Index;

            public int GetHashCode(SimilarityObject obj) => obj.GetHashCode();
        }

        private const short RecommendedMovieLimit = 10;
        private readonly DbContextOptions<Database.DatabaseContext> _dbContextOptions;
        private readonly PythonEngineOptions _options;
        private readonly PythonRecommendationEngineCache _cache;
        private IEnumerable<Database.Tag> _tags = null;

        private IEnumerable<Database.Tag> Tags
        {
            get
            {
                if (_tags is null)
                {
                    var context = new Database.DatabaseContext(_dbContextOptions);
                    _tags = context.Tags.OrderBy(t => t.Id);
                }

                return _tags;
            }
        }

        public PythonRecommendationEngine(DbContextOptions<Database.DatabaseContext> dbContextOptions, PythonEngineOptions options)
        {
            _options = options;
            _dbContextOptions = dbContextOptions;
            if (!PythonEngine.IsInitialized)
            {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }
            _cache = new PythonRecommendationEngineCache(options);
        }

        public async Task<int> GenerateRecommendation(RecommendationParameters parameters)
        {
            var context = new Database.DatabaseContext(_dbContextOptions);

            var recommendedMovieIds = await GetRecommendedMovieIds(context, parameters);

            var recommendation = new Database.Recommendation()
            {
                Date = DateTime.Now,
                UserId = parameters.UserId
            };
            context.Add(recommendation);
            await context.SaveChangesAsync();

            var recommendedMovies = recommendedMovieIds.Select(rm => new Database.RecommendedMovie()
            {
                RecommendationId = recommendation.Id,
                MovieId = rm,
                PossibleRating = 0.0f,
            });

            context.AddRange(recommendedMovies);
            await context.SaveChangesAsync();

            return 0;
        }

        private async Task<IEnumerable<int>> GetRecommendedMovieIds(Database.DatabaseContext context, RecommendationParameters parameters)
        {
            if (!_cache.IsPopulated)
                await PrepareData();

            var movieIds = _cache.RetrieveMovieIdsFromCache();
            var similarityMatrix = _cache.RetrieveSimilarityMatrixFromCache(movieIds);

            var userMovies = GetUserMoviesByGenres(context, parameters);

            if (userMovies.Count() == 0)
                userMovies = GetUserMovies(context);

            if (userMovies.Count() == 0)
                return GetMostPopularMovies(context, parameters);

            // Limit matrix to only include requested tags and exclude user movies
            var isSimilarityAllowedFilters = CreateSimilarityMatrixFilter(similarityMatrix, movieIds, context, parameters, userMovies);

            // Gather the most similar movies
            var allSimilarities = new List<SimilarityObject>();
            foreach (var userMovie in userMovies)
            {
                var movieIndex = FindMovieIndex(movieIds, userMovie.Id);

                // Weight similarities by how good the movies are rated and include indicies in selection
                var similarities = similarityMatrix[movieIndex]
                    .Select((s, index) => new SimilarityObject { Similarity = s * (userMovie.Rating / 10), Index = index })
                    .Where(s => isSimilarityAllowedFilters[s.Index])
                    .OrderByDescending(s => s.Similarity)
                    .Take(5);

                allSimilarities.AddRange(similarities);
            }

            var equalityComparer = new SimilarityObjectComparer();

            // Get top recommended movie ids
            var userMoviesIds = userMovies.Select(um => um.Id);
            return allSimilarities
                .Distinct(equalityComparer)
                .Where(id => !userMoviesIds.Contains(id.Index))
                .OrderByDescending(s => s.Similarity)
                .Take(RecommendedMovieLimit)
                .Select(s => movieIds[s.Index]);
        }

        private IQueryable<UserMovie> GetUserMovies(Database.DatabaseContext context) =>
            context.UserMovies
                .Select(um => new UserMovie { Id = um.MovieId, Rating = um.Rating });

        private IQueryable<UserMovie> GetUserMoviesByGenres(Database.DatabaseContext context, RecommendationParameters parameters) =>
            context.UserMovies.Include(um => um.Movie.Tags)
                .Where(um => um.UserId == parameters.UserId
                    && DoesMovieContainTags(um.Movie.Tags.Select(t => t.TagId), parameters.RequestedTagIds))
                .Select(um => new UserMovie { Id = um.MovieId, Rating = um.Rating });

        private IQueryable<int> GetMostPopularMovies(Database.DatabaseContext context, RecommendationParameters parameters) =>
            context.Movies.Include(m => m.Tags)
                .Where(m => DoesMovieContainTags(m.Tags.Select(t => t.TagId), parameters.RequestedTagIds))
                .Select(m => m.Id)
                .Take(RecommendedMovieLimit);

        private bool[] CreateSimilarityMatrixFilter(
            double[][] similarityMatrix, int[] movieIds, Database.DatabaseContext context, RecommendationParameters parameters, IEnumerable<UserMovie> userMovies)
        {
            var movies = context.Movies.Include(m => m.Tags).ToArray();
            var isSimilarityAllowedFilters = new bool[similarityMatrix.GetLength(0)];
            var userMovieIds = userMovies.Select(um => um.Id);

            for (int i = 0; i < movieIds.Length; i++)
            {
                var movie = movies.First(m => m.Id == movieIds[i]);

                isSimilarityAllowedFilters[i] = DoesMovieContainTags(movie, parameters.RequestedTagIds) && !userMovieIds.Contains(movieIds[i]);
            }

            return isSimilarityAllowedFilters;
        }
        private bool DoesMovieContainTags(Database.Movie movie, IEnumerable<int> requestedTagIds) =>
            DoesMovieContainTags(movie.Tags.Select(t => t.TagId), requestedTagIds);

        private bool DoesMovieContainTags(IEnumerable<int> movieTagIds, IEnumerable<int> requestedTagIds) =>
            movieTagIds.Intersect(requestedTagIds).Any();


        private int FindMovieIndex(int[] movieIds, int movieId)
        {
            for (int i = 0; i < movieIds.Length; i++)
            {
                if (movieIds[i] == movieId)
                {
                    return i;
                }
            }

            return -1;
        }

        public async Task PrepareData()
        {
            var (similarityMatrix, ids) = await FindSimilarities();

            if (!Directory.Exists(_options.RecommendationCacheLocation))
            {
                Directory.CreateDirectory(_options.RecommendationCacheLocation);
            }

            _cache.CacheSimilarityMatrix(similarityMatrix);
            _cache.CacheMovieIdArray(ids);
        }

        public async Task<(double[,] similarityMatrix, int[] ids)> FindSimilarities()
        {
            var context = new Database.DatabaseContext(_dbContextOptions);
            var movies = await context.Movies.Include(m => m.Tags).ToListAsync();

            var movieVectorizer = new MovieVectorizer(Tags);
            var weights = new MovieVectorizer.Weights
            {
                Year = 1.0f,
                Tags = 1.0f,
                Rating = 1.0f,
                Descriptions = 1.0f,
                Creators = 1.0f
            };

            var vectorizedMovies = await movieVectorizer.Vectorize(movies, weights);

            var stringifiedMatrix = Matrix.MatrixToString(vectorizedMovies);

            var similarityMatrix = await PythonMethods.FindSimilarities(stringifiedMatrix);
            var ids = movies.Select(m => m.Id).ToArray();

            return (similarityMatrix, ids);
        }
    }
}

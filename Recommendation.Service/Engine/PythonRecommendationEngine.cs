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
        struct SimilarityObject
        {
            public int Index { get; set; }
            public double Similarity { get; set; }
        }

        private readonly DbContextOptions<Database.DatabaseContext> _dbContextOptions;
        private readonly PythonEngineOptions _options;
        private readonly PythonRecommendationEngineCache _cache;
        private IEnumerable<Database.Tag> _tags = null;

        private IEnumerable<Database.Tag> Tags {
            get {
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

            var recommendedMovieIds = (await GetRecommendedMovieIds(context, parameters));

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

        private async Task<IEnumerable<int>> GetRecommendedMovieIds (Database.DatabaseContext context, RecommendationParameters parameters)
        {
            var movieIds = _cache.RetrieveMovieIdsFromCache();
            var similarityMatrix = _cache.RetrieveSimilarityMatrixFromCache(movieIds);

            var movies = await context.Movies.Include(m => m.Tags).ToListAsync();

            // Get user movies that will be used to find recommendations
            var userMovies = context.UserMovies.Include(um => um.Movie.Tags)
                .Where(um => um.UserId == parameters.UserId
                    && DoesMovieContainTags(um.Movie.Tags.Select(t => t.TagId), parameters.RequestedTagIds))
                .Select(um => new { Id = um.MovieId, Rating = um.Rating });

            // Fill user movies up if there are not enough of them
            var allMovies = movies
                .Where(m => DoesMovieContainTags(m.Tags.Select(t => t.TagId), parameters.RequestedTagIds))
                .OrderByDescending(m => m.AverageRating)
                .Select(m => new { Id = m.Id, Rating = (float)m.AverageRating })
                .Take(Math.Max(0, 10 - userMovies.Count()))
                .Union(userMovies);

            // Limit matrix to only include requested tags
            var isSimilarityAllowedFilters = CreateSimilarityMatrixFilter(similarityMatrix, movieIds, movies, parameters);

            // Gather the most similar movies
            var allSimilarities = new List<SimilarityObject>();
            foreach (var item in allMovies)
            {
                var movieIndex = FindMovieIndex(movieIds, item.Id);

                // Weight similarities by how good the movies are rated and include indicies in selection
                var similarities = similarityMatrix[movieIndex]
                    .Select((s, index) => new SimilarityObject { Similarity = s * (item.Rating / 10), Index = index })
                    .Where(s => isSimilarityAllowedFilters[s.Index])
                    .OrderByDescending(s => s.Similarity)
                    .Take(5);

                allSimilarities.AddRange(similarities);
            }

            // Get top recommended movie ids
            var userMoviesIds = userMovies.Select(um => um.Id);
            return allSimilarities
                .Where(id => !userMoviesIds.Contains(id.Index))
                .OrderByDescending(s => s.Similarity)
                .Take(10)
                .Select(s => movieIds[s.Index]);
        }

        private bool[] CreateSimilarityMatrixFilter(double[][] similarityMatrix, int[] movieIds, IEnumerable<Database.Movie> movies, RecommendationParameters parameters)
        {
            var isSimilarityAllowedFilters = new bool[similarityMatrix.GetLength(0)];
            for (int i = 0; i < movieIds.Length; i++)
            {
                var movieTagIds = movies.First(m => m.Id == movieIds[i]).Tags.Select(t => t.TagId);

                isSimilarityAllowedFilters[i] = DoesMovieContainTags(movieTagIds, parameters.RequestedTagIds);
            }
            return isSimilarityAllowedFilters;
        }

        private bool DoesMovieContainTags(IEnumerable<int> movieTagIds, IEnumerable<int> requestedTagIds)
        {
            foreach (var id in requestedTagIds)
            {
                if (!movieTagIds.Contains(id))
                    return false;
            }
            return true;
        }

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

        public async Task<IEnumerable<IEnumerable<string>>> FindDescriptionKeywords()
        {
            var context = new Database.DatabaseContext(_dbContextOptions);
            var movieDescriptions = await context.Movies
                .Select(m => m.Description == null ? "" : m.Description.ToLower())
                .ToListAsync();

            return await PythonMethods.FindKeywords(movieDescriptions);
        }

        public async Task<(double[,] similarityMatrix, int[] ids)>  FindSimilarities()
        {
            var context = new Database.DatabaseContext(_dbContextOptions);
            var movies = await context.Movies.Include(m => m.Tags).ToListAsync();

            var descriptionKeywords = await FindDescriptionKeywords();
            var descriptionData = descriptionKeywords.Select(d => d.Aggregate("", (w1, w2) => w1 + " " + w2));

            var descriptionMatrix = await VectorizeDescriptions(descriptionData);
            var movieMatrix = CreateMovieMatrix(movies);

            var joinedMatrix = Matrix.JoinMatrices(movieMatrix, Matrix.CastMatrix(descriptionMatrix));

            var stringifiedMatrix = Matrix.MatrixToString(joinedMatrix);

            var similarityMatrix = await PythonMethods.FindSimilarities(stringifiedMatrix);
            var ids = movies.Select(m => m.Id).ToArray();

            return (similarityMatrix, ids);
        }

        public async Task<long[,]> VectorizeDescriptions()
        {
            var context = new Database.DatabaseContext(_dbContextOptions);
            var descriptions = await context.Movies.Select(m => m.Description).ToListAsync();

            return await VectorizeDescriptions(descriptions);
        }

        public async Task<long[,]> VectorizeDescriptions(IEnumerable<string> descriptions)
        {
            var vectorizedDescriptions = await PythonMethods.VectorizeDocuments(descriptions);

            return FilterVectorizedDescriptions(vectorizedDescriptions, 5, 90);
        }

        private long[,] FilterVectorizedDescriptions(long[,] descriptions, float lowerPercentage, float upperPercentage)
        {
            var markedColumns = new List<int>();
            var totalEntries = descriptions.GetLength(0);
            for (int j = 0; j < descriptions.GetLength(1); j++)
            {
                var entries = 0;

                for (int i = 0; i < totalEntries; i++)
                {
                    if (descriptions[i, j] > 0)
                    {
                        descriptions[i, j] = 1;
                        entries++;
                    }      
                }

                var entryPercentage = (entries / (float)totalEntries) * 100;
                if (lowerPercentage <= entryPercentage && entryPercentage <= upperPercentage)
                {
                    markedColumns.Add(j);
                }
            }

            var newDescriptions = new long[totalEntries, markedColumns.Count];

            int index = 0;
            foreach (var columnIndex in markedColumns)
            {
                for (int rowIndex = 0; rowIndex < totalEntries; rowIndex++)
                {
                    newDescriptions[rowIndex, index] = descriptions[rowIndex, columnIndex];
                }
                index++;
            }

            return newDescriptions;
        }

        private double NormalizeAverageRating(double rating, float weight = 1.0f)
        {
            // Rating normalized to be [0;1]
            return (rating / 10.0f ) * weight;
        }

        private double NormalizeYear(int year, float weight = 1.0f)
        {
            // How old is the movie, if less than 50 y old, normalized to be [0;1]
            return ((DateTime.Now.Year - year) / 50.0f) * weight;
        }

        public double[,] CreateMovieMatrix(IEnumerable<Database.Movie> movies)
        {
            var matrix = new double[movies.Count(), 2];

            for (int i = 0; i < movies.Count(); i++)
            {
                var movie = movies.ElementAt(i);
                matrix[i, 0] = NormalizeAverageRating(movie.AverageRating);
                matrix[i, 1] = NormalizeYear(movie.Date.Year);
            }

            var tagMatrix = CreateTagMatrix(movies);

            return Matrix.JoinMatrices(matrix, tagMatrix);
        }

        private double[,] CreateTagMatrix(IEnumerable<Database.Movie> movies, float weight = 1.0f)
        {
            var rowCount = movies.Count();
            var columnCount = Tags.Count();
            var matrix = new double[movies.Count(), Tags.Count()];

            for (int i = 0; i < rowCount; i++)
            {
                var movie = movies.ElementAt(i);

                if (movie.Tags is null)
                    continue;

                var tagsIds = Tags.Select(t => t.Id).ToArray();

                for (int j = 0; j < columnCount; j++)
                {
                    var tagId = tagsIds[j];
                    matrix[i, j] = movie.Tags.FirstOrDefault(mt => mt.TagId == tagId) is null ? 0 : weight;
                }
            }

            return matrix;
        }
    }
}

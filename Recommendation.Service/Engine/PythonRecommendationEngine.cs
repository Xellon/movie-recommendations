using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Python.Runtime;

namespace Recommendation.Service
{
    public class PythonRecommendationEngine : IRecommendationEngine
    {
        private readonly DbContextOptions<Database.DatabaseContext> _dbContextOptions;

        private IEnumerable<Database.Tag> _tags = null;

        private IEnumerable<Database.Tag> Tags {
            get {
                if (_tags is null)
                {
                    var context = new Database.DatabaseContext(_dbContextOptions);
                    _tags = context.Tags;
                }

                return _tags;
            }
        }

        public PythonRecommendationEngine(DbContextOptions<Database.DatabaseContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
            if (!PythonEngine.IsInitialized)
            {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }
        }

        public async Task<int> GenerateRecommendation(RecommendationParameters parameters)
        {
            

            return 0;
        }

        public async Task<IEnumerable<IEnumerable<string>>> FindKeywords()
        {
            var context = new Database.DatabaseContext(_dbContextOptions);
            var movies = context.Movies;

            return await PythonMethods.FindKeywords(movies.Select(m => m.Description.ToLower()));
        }

        public async Task<double[,]> FindSimilarities()
        {
            var context = new Database.DatabaseContext(_dbContextOptions);
            var movies = context.Movies;

            var stringifiedMatrix = MovieMatrixToString(movies);

            return await PythonMethods.FindSimilarities(stringifiedMatrix);
        }

        public string MovieMatrixToString(IEnumerable<Database.Movie> movies)
        {
            var matrixString = "";

            foreach (var movie in movies)
            {
                matrixString += string.Format("{0} {1} {2}; ",
                    movie.AverageRating,
                    movie.Date.Year,
                    MapTags(movie.Tags));
            }

            return matrixString.Trim(' ').Trim(';');
        }

        string MapTags(IEnumerable<Database.MovieTag> tags)
        {
            if (tags is null)
                return "";

            var tagString = "";
            foreach (var tag in Tags)
            {
                tagString += tags.FirstOrDefault(t => t.TagId == tag.Id) is null  ? "0 " : "1 ";
            }
            return tagString.TrimEnd();
        }
    }
}

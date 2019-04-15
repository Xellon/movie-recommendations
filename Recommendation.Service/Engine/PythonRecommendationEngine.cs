using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Python.Runtime;
using System;
using System.Globalization;

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
                    _tags = context.Tags.OrderBy(t => t.Id);
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
            var movies = context.Movies.Include(m => m.Tags);

            var stringifiedMatrix = MovieMatrixToString(movies);

            return await PythonMethods.FindSimilarities(stringifiedMatrix);
        }

        private string FormatDouble(double number) => number.ToString(new CultureInfo("en-US"));

        private string FormatAverageRating(double rating, float weight = 1.0f)
        {
            // Rating normalized to be [0;1]
            return FormatDouble((rating / 10.0f ) * weight);
        }

        private string FormatYear(int year, float weight = 1.0f)
        {
            // How old is the movie, if less than 50 y old, normalized to be [0;1]
            return FormatDouble(((DateTime.Now.Year - year) / 50.0f) * weight);
        }

        public string MovieMatrixToString(IEnumerable<Database.Movie> movies)
        {
            var matrixString = "";

            foreach (var movie in movies)
            {
                matrixString += string.Format("{0} {1} {2}; ",
                    FormatAverageRating(movie.AverageRating), 
                    FormatYear(movie.Date.Year), 
                    MapTags(movie.Tags));
            }

            return matrixString.Trim(' ').Trim(';');
        }

        string MapTags(IEnumerable<Database.MovieTag> tags, float weight = 1.0f)
        {
            if (tags is null)
                return "";

            var tagString = "";
            foreach (var tag in Tags)
            {
                tagString += tags.FirstOrDefault(t => t.TagId == tag.Id) is null  ? "0 " : $"{FormatDouble(weight)} ";
            }
            return tagString.TrimEnd();
        }
    }
}

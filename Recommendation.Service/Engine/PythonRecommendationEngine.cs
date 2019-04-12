using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Python.Runtime;

namespace Recommendation.Service
{
    public class PythonRecommendationEngine : IRecommendationEngine
    {
        private readonly DbContextOptions<Database.DatabaseContext> _dbContextOptions;
        private readonly IConfiguration _configuration;

        public PythonRecommendationEngine(DbContextOptions<Database.DatabaseContext> dbContextOptions, IConfiguration configuration)
        {
            _dbContextOptions = dbContextOptions;
            _configuration = configuration;
        }

        public async Task<int> GenerateRecommendation(RecommendationParameters parameters)
        {
            

            return 0;
        }

        public async Task<int> FindKeywords()
        {
            var context = new Database.DatabaseContext(_dbContextOptions);
            var movies = context.Movies;

            try
            {
                using (Py.GIL())
                {
                    dynamic rake_nltk = Py.Import("rake_nltk");
                    var keywords = new List<string[]>();

                    foreach (var description in movies.Select(m => m.Description.ToLower()))
                    {
                        dynamic rake = rake_nltk.Rake();
                        rake.extract_keywords_from_text(description);
                        PyList keys = PyList.AsList(rake.get_word_degrees().keys());
                        keywords.Add((string[])keys.AsManagedObject(typeof(string[])));
                    }
                }
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }
    }
}

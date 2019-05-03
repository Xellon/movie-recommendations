using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Python.Runtime;

namespace Recommendation.Service.Tests.Unit
{
    [TestClass]
    public class PythonRecommendationEngine
    {
        private readonly PythonEngineOptions _engineOptions;

        private DbContextOptions<Database.DatabaseContext> PrepareDatabaseOptions()
        {
            var guid = Guid.NewGuid();
            var optionsBuilder = new DbContextOptionsBuilder<Database.DatabaseContext>();
            optionsBuilder.UseInMemoryDatabase(guid.ToString());

            return optionsBuilder.Options;
        }

        private Database.DatabaseContext PrepareDatabase(DbContextOptions<Database.DatabaseContext> options)
        {
            var context = new Database.DatabaseContext(options);
            context.Movies.Add(new Database.Movie
            {
                Id = 5,
                Title = "Avengers",
                Description = "Hello and welcome to Jackass"
            });
            context.Movies.Add(new Database.Movie
            {
                Id = 4,
                Title = "THE Avengers",
                Description = "Welcome to hype machine"
            });
            context.SaveChanges();

            return context;
        }

        private (DbContextOptions<Database.DatabaseContext>, Database.DatabaseContext) PrepareDatabaseAndOptions()
        {
            var contextOptions = PrepareDatabaseOptions();
            var context = PrepareDatabase(contextOptions);

            return (contextOptions, context);
        }

        public PythonRecommendationEngine()
        {
            _engineOptions = new PythonEngineOptions
            {
                RecommendationCacheLocation = "./TestCache"
            };
        }

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            if (!PythonEngine.IsInitialized)
            {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }
        }

        [TestMethod]
        public async Task FindDescriptionKeywords_RegularText_FiltersOutIrrelevantWords()
        {
            (var contextOptions, var context) = PrepareDatabaseAndOptions();

            var engine = new Service.PythonRecommendationEngine(contextOptions, _engineOptions);
            var keywordLists = (await engine.FindDescriptionKeywords()).Select(keywords => keywords.OrderBy(word => word));

            var serializedKeywordLists = JsonConvert.SerializeObject(keywordLists);

            Assert.AreEqual("[[\"hello\",\"jackass\",\"welcome\"],[\"hype\",\"machine\",\"welcome\"]]", serializedKeywordLists);
        }

        [TestMethod]
        public async Task FindSimilarities_FourMovies_GeneratesCorrectSimilarityMatrix()
        {
            var contextOptions = PrepareDatabaseOptions();

            var context = new Database.DatabaseContext(contextOptions);

            context.Movies.Add(new Database.Movie
            {
                Id = 1,
                Date = new DateTime(2017, 1, 1),
                AverageRating = 8,
                Tags = new List<Database.MovieTag>()
                {
                    new Database.MovieTag {MovieId = 1, TagId = 2},
                    new Database.MovieTag {MovieId = 1, TagId = 4},
                    new Database.MovieTag {MovieId = 1, TagId = 8},
                }
            });
            context.Movies.Add(new Database.Movie
            {
                Id = 2,
                Date = new DateTime(2016, 1, 1),
                AverageRating = 7,
                Tags = new List<Database.MovieTag>()
                {
                    new Database.MovieTag {MovieId = 2, TagId = 2},
                    new Database.MovieTag {MovieId = 2, TagId = 8},
                }
            });
            context.Movies.Add(new Database.Movie
            {
                Id = 3,
                Date = new DateTime(2012, 1, 1),
                AverageRating = 9,
                Tags = new List<Database.MovieTag>()
                {
                    new Database.MovieTag {MovieId = 3, TagId = 4},
                }
            });
            context.Movies.Add(new Database.Movie
            {
                Id = 4,
                Date = new DateTime(1988, 1, 1),
                AverageRating = 5,
                Tags = new List<Database.MovieTag>()
                {
                    new Database.MovieTag {MovieId = 4, TagId = 4},
                }
            });
            context.Tags.Add(new Database.Tag() { Id = 2, Text = "A" });
            context.Tags.Add(new Database.Tag() { Id = 4, Text = "B" });
            context.Tags.Add(new Database.Tag() { Id = 8, Text = "C" });
            context.SaveChanges();

            var engine = new Service.PythonRecommendationEngine(contextOptions, _engineOptions);
            var (similaritiesMatrix, ids) = await engine.FindSimilarities();

            var similarityMatrixString = JsonConvert.SerializeObject(similaritiesMatrix);
            var idsString = JsonConvert.SerializeObject(ids);
            Assert.AreEqual(
                "[[1.0,0.85032982259575962,0.66852235756773826,0.584021194957838],[0.85032982259575962,1.0,0.298883133547568,0.1917974265929025],[0.66852235756773826,0.298883133547568,1.0000000000000002,0.88871006494571314],[0.584021194957838,0.1917974265929025,0.88871006494571314,1.0000000000000002]]", 
                similarityMatrixString);
            Assert.AreEqual("[1,2,3,4]", idsString);
        }

        [TestMethod]
        public void CreateMovieMatrix_TwoMovies_VectorizesMovieAttributesCorrectly()
        {
            var contextOptions = PrepareDatabaseOptions();

            var context = new Database.DatabaseContext(contextOptions);

            context.Movies.Add(new Database.Movie
            {
                Id = 1,
                Date = DateTime.Now,
                AverageRating = 8,
                Tags = new List<Database.MovieTag>()
                {
                    new Database.MovieTag {MovieId = 1, TagId = 2},
                    new Database.MovieTag {MovieId = 1, TagId = 4},
                    new Database.MovieTag {MovieId = 1, TagId = 8},
                }
            });
            context.Movies.Add(new Database.Movie
            {
                Id = 2,
                Date = DateTime.Now,
                AverageRating = 7,
                Tags = new List<Database.MovieTag>()
                {
                    new Database.MovieTag {MovieId = 2, TagId = 2},
                    new Database.MovieTag {MovieId = 2, TagId = 8},
                }
            });
            context.Tags.Add(new Database.Tag() { Id = 2, Text = "A" });
            context.Tags.Add(new Database.Tag() { Id = 4, Text = "B" });
            context.Tags.Add(new Database.Tag() { Id = 8, Text = "C" });
            context.SaveChanges();

            var engine = new Service.PythonRecommendationEngine(contextOptions, _engineOptions);
            var matrixString = JsonConvert.SerializeObject(engine.CreateMovieMatrix(context.Movies));
            Assert.AreEqual("[[0.8,0.0,1.0,1.0,1.0],[0.7,0.0,1.0,0.0,1.0]]", matrixString);
        }
    }
}

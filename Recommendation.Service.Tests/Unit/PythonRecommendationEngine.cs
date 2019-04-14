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
        public async Task FindKeywords()
        {
            (var contextOptions, var context) = PrepareDatabaseAndOptions();

            var engine = new Service.PythonRecommendationEngine(contextOptions);
            var keywordLists = (await engine.FindKeywords()).Select(keywords => keywords.OrderBy(word => word));

            var serializedKeywordLists = JsonConvert.SerializeObject(keywordLists);

            context.Dispose();
            Assert.AreEqual("[[\"hello\",\"jackass\",\"welcome\"],[\"hype\",\"machine\",\"welcome\"]]", serializedKeywordLists);
        }

        [TestMethod]
        public void MovieMatrixToString()
        {
            var contextOptions = PrepareDatabaseOptions();

            var context = new Database.DatabaseContext(contextOptions);

            context.Movies.Add(new Database.Movie
            {
                Id = 1,
                Date = DateTime.Now,
                AverageRating = 8,
                Tags = new List<Database.MovieTag> ()
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

            var engine = new Service.PythonRecommendationEngine(contextOptions);
            var matrixString = engine.MovieMatrixToString(context.Movies);

            context.Dispose();
            Assert.AreEqual("8 2019 1 1 1; 7 2019 1 0 1", matrixString);
        }
    }
}

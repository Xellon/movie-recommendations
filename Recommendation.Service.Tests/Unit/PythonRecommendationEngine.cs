﻿using Microsoft.EntityFrameworkCore;
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
                "[[0.99999999999999989,0.82635475703954275,0.600537953666761,0.4680647952396943]," +
                "[0.82635475703954275,1.0,0.10823907133936798,0.084482520571145511]," +
                "[0.600537953666761,0.10823907133936798,1.0000000000000002,0.84370737503951609]," +
                "[0.4680647952396943,0.084482520571145511,0.84370737503951609,1.0]]", 
                similarityMatrixString);
            Assert.AreEqual("[1,2,3,4]", idsString);
        }
    }
}

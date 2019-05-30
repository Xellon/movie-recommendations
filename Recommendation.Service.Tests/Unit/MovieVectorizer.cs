using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Recommendation.Service.Tests.Unit
{
    [TestClass]
    public class MovieVectorizer
    {
        IEnumerable<Database.Movie> CreateMovies() => new List<Database.Movie>
            {
                new Database.Movie
                {
                    Id = 1,
                    Date = DateTime.Now,
                    AverageRating = 8,
                    Tags = new List<Database.MovieTag>()
                    {
                        new Database.MovieTag {MovieId = 1, TagId = 2},
                        new Database.MovieTag {MovieId = 1, TagId = 4},
                        new Database.MovieTag {MovieId = 1, TagId = 8},
                    },
                    Creators = new List<Database.MovieCreator>() { new Database.MovieCreator { CreatorId = 1, MovieId = 1 } }
                },
                new Database.Movie
                {
                    Id = 2,
                    Date = DateTime.Now,
                    AverageRating = 7,
                    Tags = new List<Database.MovieTag>()
                    {
                        new Database.MovieTag {MovieId = 2, TagId = 2},
                        new Database.MovieTag {MovieId = 2, TagId = 8},
                    },
                    Creators = new List<Database.MovieCreator>() { new Database.MovieCreator { CreatorId = 3, MovieId = 2 } }
                }
            };

        IEnumerable<Database.Tag> CreateTags() => new List<Database.Tag>
            {
                new Database.Tag() { Id = 2, Text = "A" },
                new Database.Tag() { Id = 4, Text = "B" },
                new Database.Tag() { Id = 8, Text = "C" }
            };
        IEnumerable<Database.Creator> CreateCreators() => new List<Database.Creator>
            {
                new Database.Creator() { Id = 1, Name = "Marvel" },
                new Database.Creator() { Id = 3, Name = "Disney" }
            };


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
        public void CreateMovieMatrix_TwoMovies_VectorizesMovieAttributesCorrectly()
        {
            var movies = CreateMovies();
            var tags = CreateTags();
            var creators = CreateCreators();

            var vectorizer = new Service.MovieVectorizer(tags, creators);

            var matrixString = JsonConvert.SerializeObject(vectorizer.CreateMovieMatrix(movies));
            Assert.AreEqual("[[0.8,0.0,1.0,1.0,1.0,1.0,0.0],[0.7,0.0,1.0,0.0,1.0,0.0,1.0]]", matrixString);
        }

        [TestMethod]
        public void CreateMovieMatrix_TwoMoviesWithCustomWeights_VectorizesMovieAttributesCorrectly()
        {
            var movies = CreateMovies();
            var tags = CreateTags();
            var creators = CreateCreators();

            var weights = new Service.MovieVectorizer.Weights()
            {
                Creators = 10.0f,
                Descriptions = 9.0f,
                Rating = 8.0f,
                Tags = 7.0f,
                Year = 6.0f
            };

            var vectorizer = new Service.MovieVectorizer(tags, creators);

            var matrixString = JsonConvert.SerializeObject(vectorizer.CreateMovieMatrix(movies, weights));
            Assert.AreEqual("[[6.4,0.0,7.0,7.0,7.0,10.0,0.0],[5.6,0.0,7.0,0.0,7.0,0.0,10.0]]", matrixString);
        }

        [TestMethod]
        public void CreateTagMatrix_ThreeTags_VectorizesTagsCorrectly()
        {
            var movies = CreateMovies();
            var tags = CreateTags();

            var vectorizer = new Service.MovieVectorizer(tags, new List<Database.Creator>());

            var matrixString = JsonConvert.SerializeObject(vectorizer.CreateTagMatrix(movies));
            Assert.AreEqual("[[1.0,1.0,1.0],[1.0,0.0,1.0]]", matrixString);
        }

        [TestMethod]
        public void CreateCreatorMatrix_TwoCreators_VectorizesCreatorsCorrectly()
        {
            var movies = CreateMovies();
            var creators = CreateCreators();

            var vectorizer = new Service.MovieVectorizer(new List<Database.Tag>(), creators);

            var matrixString = JsonConvert.SerializeObject(vectorizer.CreateCreatorMatrix(movies));
            Assert.AreEqual("[[1.0,0.0],[0.0,1.0]]", matrixString);
        }

        [TestMethod]
        public async Task VectorizeDescriptions_NoRepeatingWords_VectorizesDescriptionsCorrectly()
        {
            var descriptions = new List<string>()
            {
                "one word after another",
                "no repeating phrases"
            };

            var vectorizer = new Service.MovieVectorizer(new List<Database.Tag>(), new List<Database.Creator>());

            var matrixString = JsonConvert.SerializeObject(await vectorizer.VectorizeDescriptions(descriptions));
            Assert.AreEqual("[[],[]]", matrixString);
        }

        [TestMethod]
        public async Task VectorizeDescriptions_SingleRepeatingWord_VectorizesDescriptionsCorrectly()
        {
            var descriptions = new List<string>()
            {
                "one word",
                "another word"
            };

            var vectorizer = new Service.MovieVectorizer(new List<Database.Tag>(), new List<Database.Creator>());

            var matrixString = JsonConvert.SerializeObject(await vectorizer.VectorizeDescriptions(descriptions));
            Assert.AreEqual("[[1.0],[1.0]]", matrixString);
        }

        [TestMethod]
        public async Task VectorizeDescriptions_SingleRepeatingWordWithWeight_VectorizesDescriptionsCorrectly()
        {
            var descriptions = new List<string>()
            {
                "one word",
                "another word"
            };

            var vectorizer = new Service.MovieVectorizer(new List<Database.Tag>(), new List<Database.Creator>());

            var matrixString = JsonConvert.SerializeObject(await vectorizer.VectorizeDescriptions(descriptions, 5));
            Assert.AreEqual("[[5.0],[5.0]]", matrixString);
        }
    }
}

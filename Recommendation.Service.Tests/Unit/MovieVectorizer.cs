﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Recommendation.Service.Tests.Unit
{
    [TestClass]
    public class MovieVectorizer
    {
        [TestMethod]
        public void CreateMovieMatrix_TwoMovies_VectorizesMovieAttributesCorrectly()
        {
            var movies = new List<Database.Movie>
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

            var tags = new List<Database.Tag>
            {
                new Database.Tag() { Id = 2, Text = "A" },
                new Database.Tag() { Id = 4, Text = "B" },
                new Database.Tag() { Id = 8, Text = "C" }
            };

            var creators = new List<Database.Creator>
            {
                new Database.Creator() { Id = 1, Name = "Marvel" },
                new Database.Creator() { Id = 3, Name = "Disney" }
            };

            var vectorizer = new Service.MovieVectorizer(tags, creators);

            var matrixString = JsonConvert.SerializeObject(vectorizer.CreateMovieMatrix(movies));
            Assert.AreEqual("[[0.8,0.0,1.0,1.0,1.0,1.0,0.0],[0.7,0.0,1.0,0.0,1.0,0.0,1.0]]", matrixString);
        }
    }
}

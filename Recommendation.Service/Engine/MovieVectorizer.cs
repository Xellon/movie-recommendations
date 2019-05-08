using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Recommendation.Service
{
    public class MovieVectorizer
    {
        public struct Weights
        {
            public float Year { get; set; }
            public float Descriptions { get; set; }
            public float Tags { get; set; }
            public float Rating { get; set; }
            public float Creators { get; set; }

        }

        private IEnumerable<Database.Tag> Tags { get; }

        public MovieVectorizer(IEnumerable<Database.Tag> tags)
        {
            Tags = tags;
        }

        public async Task<double[,]> Vectorize(IEnumerable<Database.Movie> movies, Weights weights)
        {
            var descriptions = movies.Select(m => m.Description);

            var descriptionMatrix = await VectorizeDescriptions(descriptions, weights.Descriptions);
            var movieMatrix = CreateMovieMatrix(movies, weights);

            return Matrix.JoinMatrices(movieMatrix, descriptionMatrix);
        }

        public async Task<double[,]> VectorizeDescriptions(IEnumerable<string> descriptions, float weight = 1.0f)
        {
            var vectorizedDescriptions = await PythonMethods.VectorizeDocumentsTFIDF(descriptions);

            return FilterVectorizedDescriptions(vectorizedDescriptions, weight, 20);
        }

        private double[,] FilterVectorizedDescriptions(double[,] descriptions, float weight = 1.0f, float cutOffPercentage = 50.0f)
        {
            var markedColumns = new List<int>();
            var totalEntries = descriptions.GetLength(0);

            for (int j = 0; j < descriptions.GetLength(1); j++)
            {
                var entries = 0;
                double sum = 0;
                for (int i = 0; i < totalEntries; i++)
                {
                    if (descriptions[i, j] > 0)
                    {
                        sum += descriptions[i, j];
                        descriptions[i, j] = weight;
                        entries++;
                    }
                }

                var entryPercentage = (sum / entries) * 100;
                if (cutOffPercentage <= entryPercentage && entries > 1)
                {
                    markedColumns.Add(j);
                }
            }

            var newDescriptions = new double[totalEntries, markedColumns.Count];

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

        public double[,] CreateMovieMatrix(IEnumerable<Database.Movie> movies)
        {
            var defaultWeights = new Weights
            {
                Creators = 1.0f,
                Descriptions = 1.0f,
                Rating = 1.0f,
                Tags = 1.0f,
                Year = 1.0f
            };

            return CreateMovieMatrix(movies, defaultWeights);
        }

        public double[,] CreateMovieMatrix(IEnumerable<Database.Movie> movies, Weights weights)
        {
            var matrix = new double[movies.Count(), 2];

            for (int i = 0; i < movies.Count(); i++)
            {
                var movie = movies.ElementAt(i);
                matrix[i, 0] = NormalizeAverageRating(movie.AverageRating, weights.Rating);
                matrix[i, 1] = NormalizeYear(movie.Date.Year, weights.Year);
            }

            var tagMatrix = CreateTagMatrix(movies, weights.Tags);

            return Matrix.JoinMatrices(matrix, tagMatrix);
        }

        private double NormalizeAverageRating(double rating, float weight = 1.0f)
        {
            // Rating normalized to be [0;1]
            return (rating / 10.0f) * weight;
        }

        private double NormalizeYear(int year, float weight = 1.0f)
        {
            // How old is the movie, if less than 50 y old, normalized to be [0;1]
            return ((DateTime.Now.Year - year) / 50.0f) * weight;
        }

        private double[,] CreateTagMatrix(IEnumerable<Database.Movie> movies, float weight = 1.0f)
        {
            var rowCount = movies.Count();
            var columnCount = Tags.Count();
            var matrix = new double[movies.Count(), Tags.Count()];
            var tagsIds = Tags.Select(t => t.Id).ToArray();

            for (int i = 0; i < rowCount; i++)
            {
                var movie = movies.ElementAt(i);

                if (movie.Tags is null)
                    continue;

                for (int j = 0; j < columnCount; j++)
                {
                    matrix[i, j] = movie.Tags.FirstOrDefault(mt => mt.TagId == tagsIds[j]) is null ? 0 : weight;
                }
            }

            return matrix;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recommendation.Service
{
    public class RecommendationEngine : IRecommendationEngine
    {
        private readonly DbContextOptions<Database.DatabaseContext> _dbContextOptions;

        public RecommendationEngine(DbContextOptions<Database.DatabaseContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        public async Task<int> GenerateRecommendation(RecommendationParameters parameters)
        {
            

            return 0;
        }

        public Task PrepareData()
        {
            throw new NotImplementedException();
        }

        public async Task<ITransformer> CreateCollaborativeModel()
        {
            var context = new Database.DatabaseContext(_dbContextOptions);
            var userMovies = await context.UserMovies.ToListAsync();
            var mlContext = new MLContext();

            var trainingDataView = mlContext.Data.LoadFromEnumerable(userMovies);

            IEstimator<ITransformer> estimator =
                mlContext.Transforms.Conversion
                    .MapValueToKey(outputColumnName: "userIdEncoded", inputColumnName: "UserId")
                .Append(mlContext.Transforms.Conversion
                    .MapValueToKey(outputColumnName: "movieIdEncoded", inputColumnName: "MovieId"));

            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "userIdEncoded",
                MatrixRowIndexColumnName = "movieIdEncoded",
                LabelColumnName = "Rating",
                NumberOfIterations = 20,
                ApproximationRank = 100
            };

            var trainerEstimator = estimator.Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));
            ITransformer model = trainerEstimator.Fit(trainingDataView);

            return model;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recommendation.Service
{
    public class SQLRecommendationEngine : IRecommendationEngine
    {
        private readonly DbContextOptions<Database.DatabaseContext> _dbContextOptions;

        public SQLRecommendationEngine(DbContextOptions<Database.DatabaseContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        public async Task<int> GenerateRecommendation(RecommendationParameters parameters)
        {
            var context = new Database.DatabaseContext(_dbContextOptions);

            var recommendedMovies = (
                from movies in context.Movies
                join tags in context.MovieTags on movies.Id equals tags.MovieId
                where parameters.RequestedTagIds.Contains(tags.TagId)
                select new { movies.Id, Rating = movies.AverageRating, }
                ).Distinct().OrderByDescending(movie => movie.Rating).Take(10);

            if (recommendedMovies.Count() == 0)
                return 0;

            var recommendation = new Database.Recommendation()
            {
                UserId = parameters.UserId,
            };

            context.Recommendations.Attach(recommendation);
            context.Recommendations.Add(recommendation);
            await context.SaveChangesAsync();

            foreach (var movie in await recommendedMovies.ToListAsync())
            {
                var recommendedMovie = new Database.RecommendedMovie()
                {
                    MovieId = movie.Id,
                    PossibleRating = movie.Rating,
                    RecommendationId = recommendation.Id,
                };

                context.RecommendedMovies.Attach(recommendedMovie);
                context.RecommendedMovies.Add(recommendedMovie);
            }

            await context.SaveChangesAsync();
            context.Dispose();

            return recommendation.Id;
        }

        public Task PrepareData()
        {
            throw new System.NotImplementedException();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recommendation.Service
{
    public class RecommendationEngine : IRecommendationEngine
    {
        private readonly Database.DatabaseContext _databaseContext;

        public RecommendationEngine(Database.DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<int> FindOutStuff(RecommendationParameters parameters)
        {
            var movies = await _databaseContext.Movies
                .Where(m => parameters.RequestedTagIds.Contains(m.Id))
                .OrderByDescending(m => m.AverageRating)
                .Take(10)
                .ToListAsync();

            var recommendation = new Database.Recommendation()
            {
                UserId = parameters.UserId,
            };

            _databaseContext.Recommendations.Add(recommendation);
            await _databaseContext.SaveChangesAsync();

            var recommendedMovies = movies.Select(m =>
                new Database.RecommendedMovie()
                {
                    MovieId = m.Id,
                    PossibleRating = m.AverageRating,
                    RecommendationId = recommendation.Id,
                });

            foreach (var movie in recommendedMovies)
            {
                _databaseContext.RecommendedMovies.Add(movie);
            }
            await _databaseContext.SaveChangesAsync();

            return recommendation.Id;
        }
    }
}

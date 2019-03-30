using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Recommendation.Database;

namespace Recommendation.Client.Controllers
{
    public partial class DataController : ControllerBase
    {
        [HttpGet("recommendedmovies")]
        public IEnumerable<RecommendedMovie> RecommendedMovies(string userId)
        {
            return _context.Recommendations.Where(r => r.UserId == userId)
                .Join(_context.RecommendedMovies,
                    r => r.Id,
                    m => m.RecommendationId,
                    (r, m) => m);
        }

        [HttpGet("recommendedmovies/{id}")]
        public IEnumerable<RecommendedMovie> RecommendedMovies([FromRoute]int id, string userId)
        {
            return _context.Recommendations.Where(r => r.UserId == userId && r.Id == id)
                .Join(_context.RecommendedMovies,
                    r => r.Id,
                    m => m.RecommendationId,
                    (r, m) => m);
        }

        [HttpGet("recommendedmovies/latest")]
        public IEnumerable<RecommendedMovie> LatestRecommendedMovies(string userId)
        {
            // TODO: change max id to max date
            var recommendations = _context.Recommendations.Where(r => r.UserId == userId);

            if (!recommendations.Any())
                return new RecommendedMovie[0];

            return _context.RecommendedMovies.Where(m => m.RecommendationId == recommendations.Max(r => r.Id));
        }
    }
}

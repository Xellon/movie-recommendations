using System.Collections.Generic;
using System.Threading.Tasks;

namespace Recommendation.Service
{
    public interface IRecommendationEngine
    {
        Task<int> GenerateRecommendation(RecommendationParameters parameters);
        Task PrepareData();
    }
}
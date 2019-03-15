using System.Collections.Generic;
using System.Threading.Tasks;

namespace Recommendation.Service
{
    public interface IRecommendationEngine
    {
        Task<int> FindOutStuff(int userId, List<int> requestedTagIds);
    }
}
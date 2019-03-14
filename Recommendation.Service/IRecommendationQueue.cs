using System.Collections.Generic;

namespace Recommendation.Service
{
    public interface IRecommendationQueue
    {
        int GetRecommendationId(int queuedRecommendationId);
        Database.RecommendationStatus GetRecommendationStatus(int queuedRecommendationId);
        int QueueRecommendation(int userId, List<int> requestedTagIds);
        void StopRecommendation(int queuedRecommendationId);
    }
}

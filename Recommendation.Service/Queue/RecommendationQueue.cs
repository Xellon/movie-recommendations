using System.Collections.Generic;
using System.Linq;

namespace Recommendation.Service
{
    public interface IRecommendationQueue
    {
        int QueueRecommendation(int userId, List<int> requestedTagIds);
        QueuedRecommendation GetUnstartedRecommendation();
    }

    public class RecommendationQueue : IRecommendationQueue
    {
        private IQueuedRecommendationStorage _storage;

        public RecommendationQueue(IQueuedRecommendationStorage storage)
        {
            _storage = storage;
        }

        public int QueueRecommendation(int userId, List<int> requestedTagIds)
        {
            var recommendationId = _storage.Add(new RecommendationParameters
            {
                UserId = userId,
                RequestedTagIds = requestedTagIds
            });

            return recommendationId;
        }

        public QueuedRecommendation GetUnstartedRecommendation()
        {
            return _storage.GetOldestUnstartedRecommendation();
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace Recommendation.Service
{
    public interface IRecommendationQueue
    {
        int QueueRecommendation(string userId, List<int> requestedTagIds);
        QueuedRecommendation GetUnstartedRecommendation();
        int QueuedCount { get; }
    }

    public class RecommendationQueue : IRecommendationQueue
    {
        private IQueuedRecommendationStorage _storage;

        public RecommendationQueue(IQueuedRecommendationStorage storage)
        {
            _storage = storage;
        }

        public int QueueRecommendation(string userId, List<int> requestedTagIds)
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
            try
            {
                return _storage.GetOldestUnstartedRecommendation();
            }
            catch
            {
                return null;
            }
        }

        public int QueuedCount {
            get {
                try
                {
                    return _storage.GetQueuedCount();
                }
                catch
                {
                    return 0;
                }

            }
        }
    }
}

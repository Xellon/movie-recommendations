using System.Collections.Generic;
using System.Linq;

namespace Recommendation.Service
{
    public class RecommendationQueue : IRecommendationQueue
    {
        private int _lastId = 0;
        private readonly List<QueuedRecommendation> _recommendations = new List<QueuedRecommendation>();
        private readonly RecommendationEngine _recommendationEngine;

        public RecommendationQueue(Database.DatabaseContext databaseContext)
        {
            _recommendationEngine = new RecommendationEngine(databaseContext);
        }

        private int GenerateNewId() => ++_lastId;

        public int GetRecommendationId(int queuedRecommendationId) => 0;

        public Database.RecommendationStatus GetRecommendationStatus(int queuedRecommendationId)
        {
            return _recommendations.FirstOrDefault(r => r.Id == queuedRecommendationId).Status;
        }

        public int QueueRecommendation(int userId, List<int> requestedTagIds)
        {
            var id = GenerateNewId();

            var task = _recommendationEngine.FindOutStuff(userId, requestedTagIds);

            var queuedRecommendation = new QueuedRecommendation(id, task);

            // Save queuedRecommendation to shared cache
            // ...
            _recommendations.Add(queuedRecommendation);

            return id;
        }

        public void StopRecommendation(int queuedRecommendationId)
        {
            _recommendations.FirstOrDefault(r => r.Id == queuedRecommendationId).Stop();
        }
    }
}

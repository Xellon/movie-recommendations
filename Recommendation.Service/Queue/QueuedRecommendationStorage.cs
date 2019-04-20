using System;
using System.Collections.Generic;
using System.Linq;

namespace Recommendation.Service
{
    public interface IQueuedRecommendationStorage
    {
        int Add(RecommendationParameters parameters);
        bool Remove(int queuedRecommendationId);
        void SetRecommendationStatus(int queuedRecommendationId, Database.RecommendationStatus newStatus);
        Database.RecommendationStatus GetRecommendationStatus(int queuedRecommendationId);
        QueuedRecommendation GetOldestUnstartedRecommendation();
        void SetRecommendationId(int queuedRecommendationId, int recommendationId);
        int GetRecommendationId(int queuedRecommendationId);
        int GetQueuedCount();
    }

    public class QueuedRecommendationStorage : IQueuedRecommendationStorage
    {
        private int _lastRecommendationId = 0;
        private List<QueuedRecommendation> _recommendations = new List<QueuedRecommendation>();

        private int GetNewRecommendationId() => ++_lastRecommendationId;

        private QueuedRecommendation Get(int queuedRecommendationId)
        {
            return _recommendations.FirstOrDefault(r => r.Id == queuedRecommendationId);
        }

        public int Add(RecommendationParameters parameters)
        {
            var recommendation = new QueuedRecommendation
            {
                Id = GetNewRecommendationId(),
                RecommendationParameters = parameters,
                Status = Database.RecommendationStatus.Queued,
                StartTime = DateTime.Now
            };

            _recommendations.Add(recommendation);

            return recommendation.Id;
        }

        public bool Remove(int queuedRecommendationId)
        {
            return _recommendations.Remove(Get(queuedRecommendationId));
        }

        /// <summary>
        /// Sets recommendation status and sets stop time to current time
        /// </summary>
        public void SetRecommendationStatus(int queuedRecommendationId, Database.RecommendationStatus newStatus)
        {
            var recommendation = Get(queuedRecommendationId);

            if (recommendation is null)
                return;

            recommendation.Status = newStatus;
            recommendation.StopTime = DateTime.Now;
        }

        public Database.RecommendationStatus GetRecommendationStatus(int queuedRecommendationId)
        {
            var recommendation = Get(queuedRecommendationId);

            if (recommendation is null)
                return Database.RecommendationStatus.DoesNotExist;

            return recommendation.Status;
        }

        public QueuedRecommendation GetOldestUnstartedRecommendation()
        {
            return _recommendations.OrderByDescending(r => r.StartTime)
                .FirstOrDefault(r => r.Status == Database.RecommendationStatus.Queued);
        }

        public void SetRecommendationId(int queuedRecommendationId, int recommendationId)
        {
            var recommendation = Get(queuedRecommendationId);

            if (recommendation is null)
                return;

            recommendation.RecommendationId = recommendationId;
        }

        public int GetRecommendationId(int queuedRecommendationId)
        {
            var recommendation = Get(queuedRecommendationId);

            if (recommendation is null)
                return 0;

            return recommendation.RecommendationId;
        }

        public int GetQueuedCount()
        {
            return _recommendations.Where(r => r.Status == Database.RecommendationStatus.Queued).Count();
        }
    }
}

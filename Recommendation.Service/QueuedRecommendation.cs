using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Recommendation.Service
{
    public class QueuedRecommendation
    {
        public int Id { get; private set; }
        public int RecommendationId { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime StopTime { get; private set; }
        public Database.RecommendationStatus Status { get; private set; }

        private Task<int> RecommendationTask { get; set; }

        public QueuedRecommendation(int id, Task<int> recommendationTask)
        {
            Id = id;
            RecommendationTask = recommendationTask;

            StartTime = DateTime.Now;

            RecommendationTask.Start();
        }

        public TaskStatus RecommendationStatus()
        {
            return RecommendationTask.Status;
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}

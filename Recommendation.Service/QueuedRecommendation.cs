using System;
using System.Threading.Tasks;

namespace Recommendation.Service
{
    public class QueuedRecommendation
    {
        public int Id { get; private set; }
        public DateTime StartTime { get; private set; }
        private Task RecommendationTask { get; set; }

        public QueuedRecommendation(int id, Task task)
        {
            Id = id;
            RecommendationTask = task;

            StartTime = DateTime.Now;

            RecommendationTask.Start();
        }

        public TaskStatus RecommendationStatus()
        {
            return RecommendationTask.Status;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Recommendation.Service
{
    public class RecommendationParameters
    {
        public int UserId { get; set; }
        public List<int> RequestedTagIds { get; set; }
    }

    public class QueuedRecommendation
    {
        public int Id { get; set; }
        public int RecommendationId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public Database.RecommendationStatus Status { get; set; }
        public RecommendationParameters RecommendationParameters { get; set; }
    }
}

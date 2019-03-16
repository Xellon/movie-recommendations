using System;
using System.Collections.Generic;
using System.Text;

namespace Recommendation.Database
{
    public enum RecommendationStatus
    {
        Queued,
        InProgress,
        Finished,
        Stopped,
        Error
    }

    public class QueuedRecommendation
    {
        public int Id { get; set; }

        public RecommendationStatus Status { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }

        public int RecommendationId { get; set; }
        public Recommendation Recommendation { get; set; }
    }
}

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
        public int Id;

        public RecommendationStatus Status;

        public DateTime StartTime;
        public DateTime StopTime;

        public int RecommendationId;
        public Recommendation Recommendation;
    }
}

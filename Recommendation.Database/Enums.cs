namespace Recommendation.Database
{
    public enum RecommendationStatus
    {
        Queued,
        InProgress,
        Finished,
        Stopped,
        Error,
        DoesNotExist
    }
}

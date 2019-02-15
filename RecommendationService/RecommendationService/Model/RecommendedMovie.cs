namespace RecommendationService.Model
{
    public class RecommendedMovie
    {
        public int RecommendationId { get; set; }
        public Recommendation Recommendation { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public double PossibleRating { get; set; }
    }
}

namespace RecommendationService.Model
{
    public class TagWhish
    {
        public int RecommendationId { get; set; }
        public Recommendation Recommendation { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}

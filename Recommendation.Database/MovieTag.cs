namespace Recommendation.Database
{
    public class MovieTag
    {
        public int MovieId { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public Movie Movie { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}

namespace Recommendation.Database
{
    public class UserMovie
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public float Rating { get; set; }
    }
}

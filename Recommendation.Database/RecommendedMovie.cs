﻿namespace Recommendation.Database
{
    public class RecommendedMovie
    {
        public int RecommendationId { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public Recommendation Recommendation { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public double PossibleRating { get; set; }
    }
}

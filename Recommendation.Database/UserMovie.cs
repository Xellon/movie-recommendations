﻿using Newtonsoft.Json;

namespace Recommendation.Database
{
    public class UserMovie
    {
        public string UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public float Rating { get; set; }
    }
}

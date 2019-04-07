using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recommendation.Database
{
    public class MovieCreator
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public int CreatorId { get; set; }
        public Creator Creator { get; set; }
    }
}
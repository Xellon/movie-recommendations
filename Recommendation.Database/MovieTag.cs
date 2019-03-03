using System;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recommendation.Database
{
    public class MovieTag
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}

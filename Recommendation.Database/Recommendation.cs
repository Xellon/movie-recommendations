using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Recommendation.Database
{
    public class Recommendation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public List<RecommendedMovie> RecommendedMovies { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public List<TagWhish> TagWhishes { get; set; }
    }
}

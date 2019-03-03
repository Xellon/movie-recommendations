using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recommendation.Database
{
    public class Movie
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Creator { get; set; }
        public string Description { get; set; }
        public double AverageRating { get; set; }
        public string ImageUrl { get; set; }
        public List<MovieTag> Tags { get; set; }
    }
}
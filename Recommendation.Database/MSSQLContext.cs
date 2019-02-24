using Microsoft.EntityFrameworkCore;

namespace Recommendation.Database
{
    public class DatabaseContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserMovie>()
                .HasKey(m => new { m.MovieId, m.UserId });

            modelBuilder.Entity<TagWhish>()
                .HasKey(w => new { w.TagId, w.Recommendation });

            modelBuilder.Entity<MovieTag>()
                .HasKey(t => new { t.TagId, t.MovieId });

            modelBuilder.Entity<RecommendedMovie>()
                .HasKey(m => new { m.MovieId, m.RecommendationId });
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TagWhish> TagWhishes { get; set; }
        public DbSet<MovieTag> MovieTags { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<RecommendedMovie> RecommendedMovies { get; set; }
        public DbSet<UserMovie> UserMovies { get; set; }
    }
}

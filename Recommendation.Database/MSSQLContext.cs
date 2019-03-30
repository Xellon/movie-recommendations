using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Recommendation.Database
{
    public class DatabaseContext : IdentityDbContext<User, IdentityRole, string>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Ignore(u => u.AccessFailedCount)
                .Ignore(u => u.ConcurrencyStamp)
                .Ignore(u => u.EmailConfirmed)
                .Ignore(u => u.LockoutEnabled)
                .Ignore(u => u.LockoutEnd)
                .Ignore(u => u.NormalizedEmail)
                .Ignore(u => u.NormalizedUserName)
                .Ignore(u => u.PhoneNumber)
                .Ignore(u => u.PhoneNumberConfirmed)
                .Ignore(u => u.TwoFactorEnabled);

            modelBuilder.Entity<UserMovie>()
                .HasKey(m => new { m.MovieId, m.UserId });

            modelBuilder.Entity<TagWhish>()
                .HasKey(w => new { w.TagId, w.RecommendationId });

            modelBuilder.Entity<MovieTag>()
                .HasKey(t => new { t.TagId, t.MovieId });

            modelBuilder.Entity<RecommendedMovie>()
                .HasKey(m => new { m.MovieId, m.RecommendationId });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TagWhish> TagWhishes { get; set; }
        public DbSet<MovieTag> MovieTags { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<RecommendedMovie> RecommendedMovies { get; set; }
        public DbSet<UserMovie> UserMovies { get; set; }
    }
}

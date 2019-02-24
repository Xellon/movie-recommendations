using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Recommendation.Database;

namespace Recommendation.Client.Controllers
{
    public partial class DataController : ControllerBase
    {
        [HttpGet("user/all")]
        public IEnumerable<User> Users()
        {
            return _context.Users;
        }

        [HttpGet("user")]
        public User GetUser(int userId)
        {
            return _context.Users.FirstOrDefault(u => u.Id == userId);
        }

        [HttpGet("user/movies")]
        public IEnumerable<UserMovie> GetUserMovies(int userId)
        {
            return _context.UserMovies.Where(m => m.UserId == userId);
        }

        [HttpPost("user/movies")]
        public IActionResult PostUserMovies([FromBody]IEnumerable<SignUpController.SentUserMovie> userMovies, int userId)
        {
            foreach (var movie in userMovies)
            {
                _context.UserMovies.Add(new UserMovie()
                {
                    UserId = userId,
                    Rating = movie.Rating,
                    MovieId = movie.MovieId
                });
            }

            try
            {
                _context.SaveChanges();
            }
            catch
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpDelete("user/movies")]
        public IActionResult DeleteUserMovies([FromBody]IEnumerable<SignUpController.SentUserMovie> userMovies, int userId)
        {
            foreach (var movie in userMovies)
            {
                var userMovie = new UserMovie()
                {
                    UserId = userId,
                    MovieId = movie.MovieId
                };

                _context.UserMovies.Attach(userMovie);
                _context.UserMovies.Remove(userMovie);
            }

            try
            {
                _context.SaveChanges();
            }
            catch
            {
                return BadRequest();
            }

            return Ok();
        }
    }
}

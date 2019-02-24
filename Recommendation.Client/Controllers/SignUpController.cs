using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Recommendation.Database;

namespace Recommendation.Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignUpController : ControllerBase
    {
        private DatabaseContext _context;

        public SignUpController(DatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Index([FromBody]SentUser user)
        {
            if (!(_context.Users.FirstOrDefault(u => u.Email == user.Email) is null))
            {
                Response.StatusCode = 400;
                return Content("User already exists");
            }

            var entry = _context.Users.Add(new User
            {
                Email = user.Email,
                HashedPassword = user.Password,
            });

            if (!(user.Movies is null))
            {
                foreach (var movie in user.Movies)
                {
                    _context.UserMovies.Add(new UserMovie
                    {
                        UserId = entry.Entity.Id,
                        MovieId = movie.MovieId,
                        Rating = movie.Rating,
                    });
                }
            }

            try
            {
                _context.SaveChanges();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return new StatusCodeResult(500);
            }

            return Ok();
        }

        public struct SentUserMovie
        {
            public int MovieId;
            public int Rating;
        }

        public struct SentUser
        {
            public string Email;
            public string Password;
            public List<SentUserMovie> Movies;
        }
    }
}
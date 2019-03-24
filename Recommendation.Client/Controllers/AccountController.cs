using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Recommendation.Database;

namespace Recommendation.Client.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(DatabaseContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<int>> Login([FromBody]LoginForm form)
        {
            var user = await _userManager.FindByEmailAsync(form.Email);
            if(await _userManager.CheckPasswordAsync(user, form.Password))
            {
                await _signInManager.SignInAsync(user, false);
                return Ok(user.Id);
            }

            return Unauthorized();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Logout(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            await _userManager.UpdateSecurityStampAsync(user);

            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<int?>> Register([FromBody]RegistrationForm form)
        {
            var user = new User()
            {
                UserName = form.Email,
                Email = form.Email
            };

            var result = await _userManager.CreateAsync(user, form.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
            }

            try
            {
                await SaveUserMovies(form.Movies, user.Id);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return new StatusCodeResult(500);
            }

            return Ok(user.Id);
        }

        private async Task SaveUserMovies(List<FormUserMovie> movies, string userId)
        {
            if (!(movies is null))
            {
                foreach (var movie in movies)
                {
                    _context.UserMovies.Add(new UserMovie
                    {
                        UserId = userId,
                        MovieId = movie.MovieId,
                        Rating = movie.Rating,
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        public struct FormUserMovie
        {
            public int MovieId;
            public int Rating;
        }

        public class LoginForm
        {
            public string Email;
            public string Password;
        }

        public class RegistrationForm : LoginForm
        {

            public List<FormUserMovie> Movies;
        }
    }
}
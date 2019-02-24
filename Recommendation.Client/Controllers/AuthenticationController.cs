using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Recommendation.Client.Controllers
{
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private Database.DatabaseContext _context;

        public AuthenticationController(Database.DatabaseContext context)
        {
            _context = context;
        }

        [HttpPost("[action]")]
        public ReturnedUser? SignIn()
        {
            var email = Request.Headers["email"].First();
            var password = Request.Headers["password"].First();

            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.HashedPassword == password);

            if (user is null)
                return null;

            return new ReturnedUser
            {
                Id = user.Id,
                Email = user.Email,
            };
        }

        public struct ReturnedUser
        {
            public int Id;
            public string Email;
        }
    }
}
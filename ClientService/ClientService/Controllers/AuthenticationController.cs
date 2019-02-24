using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ClientService.Controllers
{
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private Model.SQLiteContext _context;

        public AuthenticationController(Model.SQLiteContext context)
        {
            _context = context;
        }

        [HttpPost("[action]")]
        public ReturnedUser? SignIn()
        {
            var email = Request.Headers["email"].First();
            var password = Request.Headers["password"].First();

            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password);

            if (user is null)
                return null;

            return new ReturnedUser
            {
                Id = user.Id,
                Email = user.Email,
                UserType = user.UserTypeId,
            };
        }

        public struct ReturnedUser
        {
            public int Id;
            public string Email;
            public int UserType;
        }
    }
}
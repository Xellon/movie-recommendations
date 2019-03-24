using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Recommendation.Database
{
    public class User : IdentityUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int Id { get; set; }
        //public string Email { get; set; }
        //public string HashedPassword { get; set; }
        public List<UserMovie> UserMovies { get; set; }
        public List<Recommendation> Recommendations { get; set; }
    }
}

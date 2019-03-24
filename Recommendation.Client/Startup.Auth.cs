using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recommendation.Client
{
    public partial class Startup
    {
        public void ConfigureAuth(IServiceCollection services)
        {
            services.AddIdentity<Database.User, IdentityRole>()
                .AddEntityFrameworkStores<Database.DatabaseContext>();

            services.Configure<IdentityOptions>(options =>
            {
                // Pasword settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
            });

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.Zero;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });
        }
    }
}

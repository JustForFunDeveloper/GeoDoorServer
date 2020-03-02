using GeoDoorServer.Areas.Identity.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GeoDoorServer.Models
{
    public class GeoDoorServer3Context 
        : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public GeoDoorServer3Context(DbContextOptions<GeoDoorServer3Context> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}

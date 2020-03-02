using System;
using GeoDoorServer.Areas.Identity.Model;
using GeoDoorServer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(GeoDoorServer.Areas.Identity.IdentityHostingStartup))]
namespace GeoDoorServer.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<GeoDoorServer3Context>(options =>
                    options.UseSqlite(
                        context.Configuration.GetConnectionString("GeoDoorServer3ContextConnection")));

                services.AddDefaultIdentity<ApplicationUser>()
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<GeoDoorServer3Context>();
            });
        }
    }
}
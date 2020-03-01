using System;
using GeoDoorServer3.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(GeoDoorServer3.Areas.Identity.IdentityHostingStartup))]
namespace GeoDoorServer3.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<GeoDoorServer3Context>(options =>
                    options.UseSqlite(
                        context.Configuration.GetConnectionString("GeoDoorServer3ContextConnection")));

                services.AddDefaultIdentity<IdentityUser>()
                    .AddEntityFrameworkStores<GeoDoorServer3Context>();
            });
        }
    }
}
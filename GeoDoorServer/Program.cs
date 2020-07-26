using System;
using System.Linq;
using GeoDoorServer.CustomService;
using GeoDoorServer.Data;
using GeoDoorServer.Models.DataModels;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GeoDoorServer
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // CreateWebHostBuilder(args)
            //     .Build()
            //     .UpdateDataDatabase<UserDbContext>()
            //     .Run();

            CreateHostBuilder(args)
                .Build()
                .UpdateDataDatabase<UserDbContext>()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static IHost UpdateDataDatabase<T>(this IHost webHost) where T : DbContext
        {
            var serviceScopeFactory = (IServiceScopeFactory) webHost.Services.GetService(typeof(IServiceScopeFactory));
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<T>();
                var dataSingleton = services.GetRequiredService<IDataSingleton>();
                try
                {
                    dbContext.GetService<IMigrator>().Migrate();
                }
                catch (Exception e)
                {
                    dataSingleton.AddErrorLog(new ErrorLog()
                    {
                        LogLevel = LogLevel.Debug,
                        MsgDateTime = DateTime.Now,
                        Message = $"{typeof(Program)}:UpdateDataDatabase: Exception => {e}"
                    });
                }

                dataSingleton.AddErrorLog(new ErrorLog()
                {
                    LogLevel = LogLevel.Debug,
                    MsgDateTime = DateTime.Now,
                    Message = $"{typeof(Program)}:UpdateDataDatabase successful!"
                });
                try
                {
                    if (!dbContext.GetService<UserDbContext>().Settings.Any())
                    {
                        Settings settings = new Settings()
                        {
                            GateTimeout = 30000,
                            AutoGateTimeout = 60000,
                            DoorOpenHabLink = "http://192.168.1.114:8080/rest/items/eg_tuer",
                            GateOpenHabLink = "http://192.168.1.114:8080/rest/items/eg_wand",
                            StatusOpenHabLink = "http://192.168.1.114:8080/rest/items/eg_tor_stat/state",
                            MaxErrorLogRows = 2000
                        };
                        dbContext.GetService<UserDbContext>().Settings.Add(settings);
                        dbContext.GetService<UserDbContext>().SaveChangesAsync();
                    }
                    
                    dataSingleton.AddErrorLog(new ErrorLog()
                    {
                        LogLevel = LogLevel.Debug,
                        MsgDateTime = DateTime.Now,
                        Message = $"{typeof(Program)}:Settings creation successful!"
                    });
                }
                catch (Exception e)
                {
                    dataSingleton.AddErrorLog(new ErrorLog()
                    {
                        LogLevel = LogLevel.Debug,
                        MsgDateTime = DateTime.Now,
                        Message = $"{typeof(Program)}:Create Settings Error: Exception => {e}"
                    });
                }
            }

            return webHost;
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using GeoDoorServer3.CustomService.Models;
using GeoDoorServer3.Models.DataModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GeoDoorServer3.CustomService
{
    // TODO: _timerValue should be configurable

    /// <summary>
    /// This service just checks every _timerValue seconds if the gate is open or not.
    /// </summary>
    internal class TimedOpenHabService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly int _timerValue = 5;
        public IServiceProvider Services { get; }
        
        public TimedOpenHabService(IServiceProvider services)
        {
            Services = services;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IScopedService>();

                scopedProcessingService.AddQueueMessage(new ErrorLog()
                {
                    LogLevel = LogLevel.Information,
                    MsgDateTime = DateTime.Now,
                    Message = "Timed Background Service is starting."
                });
            }

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(_timerValue));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IScopedService>();

                string result = await scopedProcessingService.GetDoorStatus();

                var scopedDataSingleton =
                    scope.ServiceProvider
                        .GetRequiredService<IDataSingleton>();

                if (result.Equals("ON"))
                    scopedDataSingleton.GetSystemStatus().GateStatus = GateStatus.GateOpen;
                else if (result.Equals("OFF"))
                    scopedDataSingleton.GetSystemStatus().GateStatus = GateStatus.GateClosed;

                scopedDataSingleton.GetSystemStatus().OnlineTimeSpan =
                    DateTime.Now.Subtract(scopedDataSingleton.GetSystemStatus().StartTime);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IScopedService>();

                scopedProcessingService.AddQueueMessage(new ErrorLog()
                {
                    LogLevel = LogLevel.Information,
                    MsgDateTime = DateTime.Now,
                    Message = "Timed Background Service is stopping."
                });
            }

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

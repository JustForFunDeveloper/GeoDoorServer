using System;
using System.Threading;
using System.Threading.Tasks;
using GeoDoorServer.Models.DataModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;
using MyThreadingTimer = System.Threading.Timer;

namespace GeoDoorServer.CustomService
{
    public class AutoGateTimeoutService : IHostedService, IDisposable
    {
        
        private readonly Timer _autoCloseTimer;
        private MyThreadingTimer _timer;
        private readonly int _timerValue = 2;

        private bool _isAutoGateEnabled = false;
        
        public IServiceProvider Services { get; }
        
        public AutoGateTimeoutService(IServiceProvider services)
        {
            Services = services;
            
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IScopedService>();

                var scopedDataSingleton =
                    scope.ServiceProvider
                        .GetRequiredService<IDataSingleton>();


                _autoCloseTimer = new Timer();
                _autoCloseTimer.Interval = scopedDataSingleton.GetSettings().AutoGateTimeout;;
                _autoCloseTimer.Elapsed += async (sender, args) =>
                {
                    _autoCloseTimer.Stop();
                    _isAutoGateEnabled = false;
                    scopedDataSingleton.SetAutoGate(false);
                    scopedDataSingleton.GetSystemStatus().IsGateMoving = true;
                    await scopedProcessingService.PostData(scopedDataSingleton.GetSettings().GateOpenHabLink, "ON", true);
                    scopedDataSingleton.GetSystemStatus().IsAutoMode = false;
                    
                    scopedProcessingService.AddQueueMessage(new ErrorLog()
                    {
                        LogLevel = LogLevel.Information,
                        MsgDateTime = DateTime.Now,
                        Message = "AutoGate Timer Ended!"
                    });
                };
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new MyThreadingTimer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(_timerValue));
            
            return Task.CompletedTask;
        }
        
        private void DoWork(object state)
        {
            using (var scope = Services.CreateScope())
            {
                
                var scopedDataSingleton =
                    scope.ServiceProvider
                        .GetRequiredService<IDataSingleton>();

                if (scopedDataSingleton.IsAutoGateEnabled() != null && (bool) scopedDataSingleton.IsAutoGateEnabled())
                {
                    if (!_isAutoGateEnabled)
                    {
                        var scopedProcessingService =
                            scope.ServiceProvider
                                .GetRequiredService<IScopedService>();

                        scopedProcessingService.AddQueueMessage(new ErrorLog()
                        {
                            LogLevel = LogLevel.Information,
                            MsgDateTime = DateTime.Now,
                            Message = "Started AutoGate Timer!"
                        });
                        
                        _autoCloseTimer.Interval = scopedDataSingleton.GetSettings().AutoGateTimeout;;
                        _autoCloseTimer.Start();
                        _isAutoGateEnabled = true;
                    }
                }
                else
                {
                    if (_isAutoGateEnabled)
                    {
                        var scopedProcessingService =
                            scope.ServiceProvider
                                .GetRequiredService<IScopedService>();

                        scopedProcessingService.AddQueueMessage(new ErrorLog()
                        {
                            LogLevel = LogLevel.Information,
                            MsgDateTime = DateTime.Now,
                            Message = "Stopped AutoGate Timer!"
                        });
                        
                        _autoCloseTimer.Stop();
                        _isAutoGateEnabled = false;
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_isAutoGateEnabled)
            {
                _autoCloseTimer.Stop();
            }
            
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer.Dispose();
            _autoCloseTimer.Dispose();
        }
    }
}
using System.Collections.Concurrent;
using GeoDoorServer.CustomService.Models;
using GeoDoorServer.Models.DataModels;

namespace GeoDoorServer.CustomService
{
    public interface IDataSingleton
    {
        public bool? IsAutoGateEnabled();
        public void SetAutoGate(bool isEnabled);
        public void SetSettings(Settings settings);
        public Settings GetSettings();
        SystemStatus GetSystemStatus();

        ConcurrentQueue<ErrorLog> GetQueue();
        void AddErrorLog(ErrorLog errorLog);
    }
}

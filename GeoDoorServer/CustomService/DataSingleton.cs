using System.Collections.Concurrent;
using GeoDoorServer.CustomService.Models;
using GeoDoorServer.Models.DataModels;

namespace GeoDoorServer.CustomService
{
    public class DataSingleton : IDataSingleton
    {
        private SystemStatus _systemStatus;
        private ConcurrentQueue<ErrorLog> _concurrentQueue;
        private Settings _settings;
        private bool? _isAutoGateEnabled;

        public DataSingleton()
        {
            _systemStatus = new SystemStatus();
            _concurrentQueue = new ConcurrentQueue<ErrorLog>();
        }
        
        public bool? IsAutoGateEnabled()
        {
            return _isAutoGateEnabled;
        }

        public void SetAutoGate(bool isEnabled)
        {
            _isAutoGateEnabled = isEnabled;
        }

        public void SetSettings(Settings settings)
        {
            _settings = settings;
            _systemStatus.GateTimeout = settings.GateTimeout;
        }

        public Settings GetSettings()
        {
            return _settings;
        }

        public SystemStatus GetSystemStatus()
        {
            return _systemStatus;
        }

        public ConcurrentQueue<ErrorLog> GetQueue()
        {
            return _concurrentQueue;
        }

        public void AddErrorLog(ErrorLog errorLog)
        {
            _concurrentQueue.Enqueue(errorLog);
        }
    }
}

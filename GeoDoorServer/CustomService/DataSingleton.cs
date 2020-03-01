using System.Collections.Concurrent;
using System.Linq;
using GeoDoorServer3.CustomService.Models;
using GeoDoorServer3.Data;
using GeoDoorServer3.Models.DataModels;

namespace GeoDoorServer3.CustomService
{
    public class DataSingleton : IDataSingleton
    {
        private SystemStatus _systemStatus;
        private ConcurrentQueue<ErrorLog> _concurrentQueue;
        //private readonly UserDbContext _context;
        
        private string _gateStatus = "";
        private string _gate = "";

        public DataSingleton()
        {
            _systemStatus = new SystemStatus();
            _concurrentQueue = new ConcurrentQueue<ErrorLog>();
        }

        public void SetGateTimeOut(int gateTimeOut)
        {
            _systemStatus.GateTimeout = gateTimeOut;
        }

        public void SetStatusGatePath(string gateStatusPath)
        {
            _gateStatus = gateStatusPath;
        }
        
        public void SetGatePath(string gatePath)
        {
            _gate = gatePath;
        }

        public string GatePathStatus()
        {
            return _gateStatus;
        }

        public string GatePathValueChange()
        {
            return _gate;
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

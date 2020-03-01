using System.Collections.Concurrent;
using GeoDoorServer3.CustomService.Models;
using GeoDoorServer3.Models.DataModels;

namespace GeoDoorServer3.CustomService
{
    public interface IDataSingleton
    {
        void SetGateTimeOut(int gateTimeOut);
        void SetStatusGatePath(string gateStatusPath);
        void SetGatePath(string gatePath);
        
        string GatePathStatus();
        string GatePathValueChange();

        SystemStatus GetSystemStatus();

        ConcurrentQueue<ErrorLog> GetQueue();
        void AddErrorLog(ErrorLog errorLog);
    }
}

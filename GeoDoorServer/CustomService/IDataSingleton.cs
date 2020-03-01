using System.Collections.Concurrent;
using GeoDoorServer.CustomService.Models;
using GeoDoorServer.Models.DataModels;

namespace GeoDoorServer.CustomService
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

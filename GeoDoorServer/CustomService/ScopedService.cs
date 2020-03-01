using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using GeoDoorServer3.CustomService.Models;
using GeoDoorServer3.Data;
using GeoDoorServer3.Models.DataModels;

namespace GeoDoorServer3.CustomService
{
    internal interface IScopedService
    {
        Task<string> GetDoorStatus();
        SystemStatus GetOpenHabStatus();
        void WriteErrorLogs();
        void AddQueueMessage(ErrorLog errorLog);
    }

    internal class ScopedService : IScopedService
    {
        private readonly IOpenHabMessageService _openHab;
        private readonly IDataSingleton _dataSingleton;
        private readonly UserDbContext _context;

        public ScopedService(IOpenHabMessageService openHab, IDataSingleton dataSingleton, UserDbContext context)
        {
            _openHab = openHab;
            _dataSingleton = dataSingleton;
            _context = context;
            
            _dataSingleton.SetGateTimeOut(_context.Settings.First(settings => settings.Id > 0).GateTimeout);
            _dataSingleton.SetStatusGatePath(_context.Settings.First(settings => settings.Id > 0).StatusOpenHabLink);
            _dataSingleton.SetGatePath(_context.Settings.First(settings => settings.Id > 0).GateOpenHabLink);
        }

        public async Task<string> GetDoorStatus()
        {
            return await _openHab.GetData(_dataSingleton.GatePathStatus());
        }

        public SystemStatus GetOpenHabStatus()
        {
            return _dataSingleton.GetSystemStatus();
        }

        public void WriteErrorLogs()
        {
            ConcurrentQueue<ErrorLog> concurrentQueue = _dataSingleton.GetQueue();

            int rowCount = _context.ErrorLogs.Count();
            int maxRows = _context.Settings.First(row => row.Id > 0).MaxErrorLogRows;
            
            if (rowCount > maxRows)
            {
                int percentCount = maxRows / 4; // 25% Count of maxRows
                int nToRemove = (rowCount - maxRows) + percentCount;
                
                var lastLogs = _context.ErrorLogs
                    .OrderBy(row => row.Id)
                    .Take(nToRemove);
                
                _context.RemoveRange(lastLogs);
            }

            foreach (var item in concurrentQueue)
            {
                if (concurrentQueue.TryDequeue(out ErrorLog errorLog))
                    _context.ErrorLogs.Add(errorLog);
                
            }
            _context.SaveChanges();
        }

        public void AddQueueMessage(ErrorLog errorLog)
        {
            _dataSingleton.AddErrorLog(errorLog);
        }
    }
}

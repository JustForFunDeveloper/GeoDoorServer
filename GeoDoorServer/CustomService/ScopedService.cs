using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using GeoDoorServer.Data;
using GeoDoorServer.Models.DataModels;

namespace GeoDoorServer.CustomService
{
    internal interface IScopedService
    {
        Task PostData(string itemName, string value, bool toggle = false);
        Task<string> GetDoorStatus();
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

            Settings settings = _context.Settings.First();
            _dataSingleton.SetSettings(settings);
        }

        public Task PostData(string itemName, string value, bool toggle = false)
        {
            return _openHab.PostData(itemName, value, toggle);
        }

        public async Task<string> GetDoorStatus()
        {
            return await _openHab.GetData(_dataSingleton.GetSettings().StatusOpenHabLink);
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using GeoDoorServer.Areas.Identity.Model;
using GeoDoorServer.CustomService;
using GeoDoorServer.CustomService.Models;
using GeoDoorServer.Data;
using GeoDoorServer.Models;
using GeoDoorServer.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GeoDoorServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserDbContext _context;
        private readonly IDataSingleton _iDataSingleton;
        private readonly IOpenHabMessageService _habMessageService;

        private int _max_Count = 100;

        private DataModel _myDataModel { get; set; }

        public HomeController(UserDbContext context, IDataSingleton dataSingleton, IOpenHabMessageService habMessageService)
        {
            _context = context;
            _iDataSingleton = dataSingleton;
            _habMessageService = habMessageService;
            _myDataModel = new DataModel(_iDataSingleton);
            
            string version = Assembly.GetEntryAssembly()?.GetName().Version.ToString();
            _myDataModel.VersionNumber = $"v{version?.Remove(version.Length - 2)}";

            List<User> users = _context.Users.OrderByDescending(u => u.LastConnection).ToList();
            _myDataModel.LastUsers = new List<string>();
            foreach (var user in users)
            {
                if (_myDataModel.LastUsers.Count <= 3)
                    _myDataModel.LastUsers.Add(user.Name);
            }

            if(users.Count > 0)
                _myDataModel.LastConnection = users[0].LastConnection;
            _myDataModel.UsersAllowed = _context.Users.Where(u => u.AccessRights.Equals(AccessRights.Allowed)).ToList().Count;
            _myDataModel.UsersRegistered = _context.Users.ToList().Count;


            _myDataModel.Errors = _context.ErrorLogs.Where(e => e.LogLevel.Equals(LogLevel.Error)).ToList().Count();
            _myDataModel.LogMessagesList = new List<string>();
            List<ErrorLog> errorLogs = _context.ErrorLogs.OrderByDescending(e => e.MsgDateTime).ToList();

            int iter = 0;
            foreach (var errorLog in errorLogs)
            {
                if (iter >= _max_Count)
                    break;
                
                _myDataModel.LogMessagesList.Add(errorLog.ToString());
                iter++;
            }
        }

        public IActionResult Index()
        {
            Response.Headers.Add("Refresh","3");
            return View(_myDataModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View(_myDataModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public class DataModel
        {
            private readonly IDataSingleton _iDataSingleton;

            public DataModel(IDataSingleton iDataSingleton)
            {
                _iDataSingleton = iDataSingleton;
            }

            public SystemStatus SystemStatus
            {
                get { return _iDataSingleton.GetSystemStatus(); }
            }

            public string OnlineTime
            {
                get
                {
                    TimeSpan timeSpan = _iDataSingleton.GetSystemStatus().OnlineTimeSpan;
                    return timeSpan.ToString(@"hh\:mm\:ss");
                }
            }

            public int UsersAllowed { get; set; }
            public int UsersRegistered { get; set; }
            public List<string> LastUsers { get; set; }
            public DateTime LastConnection { get; set; }
            public int Errors { get; set; }

            public List<string> LogMessagesList { get; set; }
            
            public string VersionNumber { get; set; }
        }
    }
}

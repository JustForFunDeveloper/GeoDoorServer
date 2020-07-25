using System.Linq;
using System.Threading.Tasks;
using GeoDoorServer.CustomService;
using GeoDoorServer.Data;
using GeoDoorServer.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeoDoorServer.Controllers
{
    public class SettingsController : Controller
    {
        private readonly UserDbContext _context;
        private readonly IDataSingleton _iDataSingleton;

        public SettingsController(UserDbContext context, IDataSingleton iDataSingleton)
        {
            _context = context;
            _iDataSingleton = iDataSingleton;
        }
        
        // GET
        public IActionResult Index()
        {
            var settings = _context.Settings.First();
            if (settings == null)
            {
                return NotFound();
            }
            
            if (User.Identity.IsAuthenticated)
                return View(settings);
            else
                return LocalRedirect("/Identity/Account/Login");
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int id, 
            [Bind("DoorOpenHabLink,GateOpenHabLink,StatusOpenHabLink,GateTimeout,MaxErrorLogRows")] Settings settings)
        {
            Settings currentSettings = _context.Settings.First();
             if (currentSettings == null)
             {
                 return NotFound();
             }

            if (ModelState.IsValid)
            {
                try
                {
                    currentSettings.DoorOpenHabLink = settings.DoorOpenHabLink;
                    currentSettings.GateOpenHabLink = settings.GateOpenHabLink;
                    currentSettings.StatusOpenHabLink = settings.StatusOpenHabLink;
                    currentSettings.GateTimeout = settings.GateTimeout;
                    currentSettings.MaxErrorLogRows = settings.MaxErrorLogRows;
                    _context.Update(currentSettings);
                    await _context.SaveChangesAsync();
                    _iDataSingleton.SetSettings(currentSettings);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(settings);
        }
    }
}
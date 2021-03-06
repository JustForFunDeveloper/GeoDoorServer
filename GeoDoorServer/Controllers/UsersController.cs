﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using GeoDoorServer.Areas.Identity.Model;
using GeoDoorServer.Data;
using GeoDoorServer.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GeoDoorServer.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserDbContext _context;
        // private readonly SignInManager<ApplicationUser> _signInManager;
        // private readonly UserManager<ApplicationUser> _userManager;
        // private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            // _signInManager = signInManager;
            // _userManager = userManager;
            // _roleManager = roleManager;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
                return View(await _context.Users.ToListAsync());
            else
                return LocalRedirect("/Identity/Account/Login");
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (User.Identity.IsAuthenticated)
                return View(user);
            return LocalRedirect("/Identity/Account/Login");
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            if (User.Identity.IsAuthenticated)
                return View();
            return LocalRedirect("/Identity/Account/Login");
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PhoneId,Name,AccessRights")] User user)
        {
            if (_context.Users.Any(u => u.PhoneId.Equals(user.PhoneId)))
                ModelState.AddModelError("PhoneId", "" +
                                                    $"PhoneId '{user.PhoneId}' does already exist!");
            if (ModelState.IsValid)
            {
                // var newApplicationUser = new ApplicationUser { UserName = user.Name, AccessRights = user.AccessRights, LastConnection = DateTime.Now};
                // var result = await _userManager.CreateAsync(newApplicationUser, "ApiAdm1n!");
                // if (result.Succeeded)
                // {
                //     var role = new IdentityRole
                //     {
                //         Name = "ApiUser"
                //     };
                //
                //     if (!await _roleManager.RoleExistsAsync(role.Name))
                //     {
                //         var roleResult = await _roleManager.CreateAsync(role);
                //         if (!roleResult.Succeeded)
                //         {
                //             return RedirectToPage("/Home/Error");
                //         }
                //     }
                //     await _userManager.AddToRoleAsync(newApplicationUser, role.Name);
                // }
                
                user.LastConnection = DateTime.Now;
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (User.Identity.IsAuthenticated)
                return View(user);
            return LocalRedirect("/Identity/Account/Login");
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,AccessRights")] User user)
        {
            User currentUser = await _context.Users.SingleOrDefaultAsync(u => u.Id.Equals(id));
            if (id != currentUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    currentUser.Name = user.Name;
                    currentUser.AccessRights = user.AccessRights;
                    _context.Update(currentUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(currentUser.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (User.Identity.IsAuthenticated)
                return View(user);
            return LocalRedirect("/Identity/Account/Login");
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
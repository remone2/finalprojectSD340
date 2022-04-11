using finalprojectSD340.Data;
using finalprojectSD340.Models;
using finalprojectSD340.HelperClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace finalprojectSD340.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly ManageUsers _mu;

        public HomeController(ApplicationDbContext db, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _mu = new ManageUsers(db, userManager, roleManager);
        }

        public async Task<IActionResult> Index()
        {
            string userName = User.Identity.Name;
            if (userName == null)
                return View();
            ApplicationUser user = await _userManager.FindByNameAsync(userName);
            List<IdentityRole> roles = _db.Roles.ToList();
            List<bool> ifInRoles = new List<bool>();
            foreach (var role in roles)
                ifInRoles.Add(await _mu.CheckForRole(user.Id, role.Id));               
            return View(ifInRoles);
        }    

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string message)
        {
            ViewBag.Message = message;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
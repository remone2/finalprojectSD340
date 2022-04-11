using finalprojectSD340.Data;
using finalprojectSD340.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using finalprojectSD340.HelperClasses;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace finalprojectSD340.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly ManageUsers _mu;

        public AdminController(ApplicationDbContext db, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _mu = new ManageUsers(db, userManager, roleManager);
        }

        public ActionResult AdminDashboard()
        {
            return View();
        }

        public async Task<IActionResult> GetAllRolesFromUser(string? id)
        {
            ViewBag.Users = new SelectList(_db.Users.ToList(), "Id", "UserName");
            if (id != null)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                ViewBag.User = user.UserName;
                IList<string> roles = await _mu.GetAllRoles(id);
                return View(roles);
            }
            return View();
        }

        public async Task<IActionResult> AssignRoleToUser()
        {
            ViewBag.Users = new SelectList(_db.Users.ToList(), "Id", "UserName");
            ViewBag.Roles = new SelectList(_db.Roles.ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AssignRoleToUser(string userId, string roleId)
        {
            ViewBag.Users = new SelectList(_db.Users.ToList(), "Id", "UserName");
            ViewBag.Roles = new SelectList(_db.Roles.ToList(), "Id", "Name");
            ViewBag.Message = await _mu.AssignRole(userId, roleId);
            return View();
        }

        public async Task<IActionResult> CheckIfUserHasRole(string? userId, string? roleId)
        {
            ViewBag.Users = new SelectList(_db.Users.ToList(), "Id", "UserName");
            ViewBag.Roles = new SelectList(_db.Roles.ToList(), "Id", "Name");
            if (userId != null && roleId != null)
            {
                bool check = await _mu.CheckForRole(userId, roleId);
                if (check)
                    ViewBag.Message = "User is already in that role";
                else
                    ViewBag.Message = "User is not in that role";
            }             
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}

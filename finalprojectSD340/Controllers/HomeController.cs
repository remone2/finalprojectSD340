using finalprojectSD340.Data;
using finalprojectSD340.Models;
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

        public HomeController(ApplicationDbContext db, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> DeveloperTasks()
        {
            try
            {
                string userName = User.Identity.Name;
                ApplicationUser dev = await _userManager.FindByNameAsync(userName);

                List<Models.Task> devTasks = _db.Tasks.Include(x => x.Project).Where(t => t.DeveloperId == dev.Id).ToList();

                return View(devTasks);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompleteTask(int taskId)
        {
            try
            {
                Models.Task task = _db.Tasks.First(t => t.Id == taskId);

                if (task == null)
                    return NotFound();

                if (task.IsCompleted)
                {
                    throw new Exception("Task is already marked complete");
                }

                if (!task.IsCompleted)
                {
                    task.IsCompleted = true;
                    task.CompletionPercentage = 100;
                }

                await _db.SaveChangesAsync();

                return RedirectToAction("DeveloperTasks");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> TaskPercentage(int percentInput, int taskId)
        {
            try
            {
                if (percentInput == null || taskId == null)
                    return BadRequest();

                Models.Task task = _db.Tasks.First(t => t.Id == taskId);

                if (task == null)
                    return NotFound();

                task.CompletionPercentage = percentInput;

                await _db.SaveChangesAsync();

                if (percentInput == 100)
                {
                    return RedirectToAction("CompleteTask", task.Id);
                }

                return RedirectToAction("DeveloperTasks");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", ex.Message);
            }
        }

        public IActionResult TaskComment()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
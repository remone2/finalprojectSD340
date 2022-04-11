using finalprojectSD340.Data;
using finalprojectSD340.Models;
using finalprojectSD340.HelperClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace finalprojectSD340.Controllers
{
    public class PMController : Controller
    {

        public ApplicationDbContext _db;
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private TaskHelper _th;
        private ManageUsers _mu;

        public PMController(ApplicationDbContext Db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = Db;
            _userManager = userManager;
            _roleManager = roleManager;
            _th = new TaskHelper(Db, userManager);
            _mu = new ManageUsers(Db, userManager, roleManager);   
        }


        [Authorize(Roles="Project Manager")]
        public IActionResult PMDashboard()
        {
            try
            {
                var Projects = _db.Projects.Include(p => p.ProjectManager)
                                       .Include(p => p.Tasks)
                                       .ThenInclude(t => t.Developer)
                                       .ToList();

                var OrderedProjects = Projects.OrderBy(p => p.Priority);

                return View(OrderedProjects);
            }
            catch
            {
                return NotFound();
            } 
        }



        [Authorize(Roles="Project Manager")]
        public IActionResult PMProjectDetails(int? id, string? filter)
        {
            try
            {
                ViewBag.Project = _db.Projects.Where(p => p.Id == id).First();

                var Tasks = _db.Tasks.Where(t => t.ProjectId == id)
                                           .Include(t => t.Developer)
                                           .Include(t => t.Project)
                                           .ThenInclude(p => p.ProjectManager)
                                           .ToList();

                var AllTasks = Tasks.OrderBy(p => p.CompletionPercentage);

                if (filter != null)
                {
                    var FilteredTasks = AllTasks.Where(t => t.IsCompleted == false);

                    return View(FilteredTasks);
                }

                return View(AllTasks);
            }
            catch
            {
                return NotFound();
            }
        }



        [Authorize(Roles = "Project Manager")]
        public IActionResult PMTasks(int? id, string? filter)
        {
            try
            {
                var Tasks = _db.Tasks.Include(t => t.Developer)
                                           .Include(t => t.Project)
                                           .ThenInclude(p => p.ProjectManager)
                                           .ToList();

                var AllTasks = Tasks.OrderBy(p => p.CompletionPercentage);

                if (filter != null)
                {
                    var FilteredTasks = AllTasks.Where(t => t.IsCompleted == false);

                    return View(FilteredTasks);
                }

                return View(AllTasks);
            }
            catch
            {
                return NotFound();
            }
        }



        [Authorize(Roles="Project Manager")]
        public IActionResult PMBudgetView()
        {
            try
            {
                var Projects = _db.Projects.Include(p => p.ProjectManager)
                                      .Include(p => p.Tasks)
                                      .ThenInclude(t => t.Developer)
                                      .ToList();

                var OrderedProjects = Projects.OrderBy(p => p.Deadline);

                return View(OrderedProjects);
            }
            catch
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> PMAddTask()
        {
            ViewBag.Projects = new SelectList(_db.Projects.ToList(), "Id", "Name");

            List<ApplicationUser> developers = new List<ApplicationUser>();
            IdentityRole developerRole = await _roleManager.FindByNameAsync("Developer");
            foreach (var user in _db.Users.ToList())
            {
                if (await _mu.CheckForRole(user.Id, developerRole.Id))
                    developers.Add(user);
            }
            ViewBag.Developers = new SelectList(developers, "Id", "UserName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PMAddTask(int projectId, string name, string desc, string? developerId, Priority priority, DateTime deadline)
        {
            ViewBag.Projects = new SelectList(_db.Projects.ToList(), "Id", "Name");

            List<ApplicationUser> developers = new List<ApplicationUser>();
            IdentityRole developerRole = await _roleManager.FindByNameAsync("Developer");
            foreach (var user in _db.Users.ToList())
            {
                if (await _mu.CheckForRole(user.Id, developerRole.Id))
                    developers.Add(user);
            }
            ViewBag.Developers = new SelectList(developers, "Id", "UserName");

            Dictionary<int, string> taskDict = await _th.Add(projectId, name, desc, developerId, priority, deadline);
            KeyValuePair<int, string> result = taskDict.First();
            if (result.Key == -1)
                return NotFound(result.Value);
            else if (result.Key == 0)
                return BadRequest(result.Value);
            ViewBag.Message = result.Value;
            return View();
        }

        public IActionResult PMDeleteTask()
        {
            return View(_db.Tasks.Include(p => p.Project).Include(d => d.Developer).ToList());
        }

        [HttpPost]
        public IActionResult PMDeleteTask(int taskId)
        {
            Dictionary<int, string> taskDict = _th.Delete(taskId);
            KeyValuePair<int, string> result = taskDict.First();
            if (result.Key == -1)
                return NotFound(result.Value);
            else if (result.Key == 0)
                return BadRequest(result.Value);
            ViewBag.Message = result.Value;
            return View(_db.Tasks.Include(p => p.Project).Include(d => d.Developer).ToList());
        }

        public async Task<IActionResult> PMAssignTask()
        {
            List<ApplicationUser> developers = new List<ApplicationUser>();
            IdentityRole developerRole = await _roleManager.FindByNameAsync("Developer");
            foreach (var user in _db.Users.ToList())
            {
                if (await _mu.CheckForRole(user.Id, developerRole.Id))
                    developers.Add(user);
            }
            ViewBag.Developers = new SelectList(developers, "Id", "UserName");
            ViewBag.Tasks = new SelectList(_db.Tasks.ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PMAssignTask(string devId, int taskId)
        {
            List<ApplicationUser> developers = new List<ApplicationUser>();
            IdentityRole developerRole = await _roleManager.FindByNameAsync("Developer");
            foreach (var user in _db.Users.ToList())
            {
                if (await _mu.CheckForRole(user.Id, developerRole.Id))
                    developers.Add(user);
            }
            ViewBag.Developers = new SelectList(developers, "Id", "UserName");
            ViewBag.Tasks = new SelectList(_db.Tasks.ToList(), "Id", "Name");

            Dictionary<int, string> taskDict = await _th.Assign(devId,taskId);
            KeyValuePair<int, string> result = taskDict.First();
            if (result.Key == -1)
                return NotFound(result.Value);
            else if (result.Key == 0)
                return BadRequest(result.Value);
            ViewBag.Message = result.Value;
            return View();
        }
    }
}

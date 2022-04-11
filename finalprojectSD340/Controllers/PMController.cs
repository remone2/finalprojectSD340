using finalprojectSD340.Data;
using finalprojectSD340.HelperClasses;
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
        private ProjectHelper _projectHelper;
        private TaskHelper _th;
        private ManageUsers _mu;


        public PMController(ApplicationDbContext Db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = Db;
            _userManager = userManager;
            _roleManager = roleManager;
            _projectHelper = new ProjectHelper(Db, userManager);
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
        public IActionResult PMProjectDetails(int? id, string? filter, string? message)
        {
            try
            {
                ViewBag.Project = _db.Projects.Where(p => p.Id == id).First();
                ViewBag.Message = message;

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


        [HttpPost]
        public async Task<IActionResult> CompleteProject(int id)
        {
            try
            {
                Project CurrentProject = _db.Projects.Include(p => p.Tasks).Where(p => p.Id == id).First();

                if (CurrentProject == null)
                {
                    return RedirectToAction("PMProjectDetails", new { id = id, message = "Could not find Project." });
                }

                if (CurrentProject.IsComplete)
                {
                    return RedirectToAction("PMProjectDetails", new { id = id, message = "Project is already marked complete" });
                }

                if (CurrentProject.Tasks.Any(t => t.IsCompleted == false))
                {
                    return RedirectToAction("PMProjectDetails", new { id = id, message = "Ensure all tasks in this project are completed" });
                }

                else
                {
                    try
                    {
                        string ProjectComplete = await _projectHelper.CompleteProject(id);

                        if (ProjectComplete == "Project completed!")
                        {
                            await _db.SaveChangesAsync();
                        }
                        else
                        {
                            throw new Exception(ProjectComplete);
                        }
                        return RedirectToAction("PMDashBoard", new { message = ProjectComplete });
                    }
                    catch (Exception ex)
                    {
                        return NotFound(ex.Message);    
                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", ex.Message);
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

        [Authorize(Roles = "Project Manager")]
        public IActionResult PMCreateProject()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PMCreateProject(string name, string desc, double budget, Priority priority, DateTime deadline)
        {
            string userName = User.Identity.Name;
            ApplicationUser? pm = await _userManager.FindByEmailAsync(userName);

            if (pm == null)
            {
                return NotFound();
            }

            Dictionary<int, string> resultDict = await _projectHelper.Add(name, desc, budget, pm.Id, priority, deadline);
            KeyValuePair<int, string> result = resultDict.First();

            if (result.Key == -1)
            {
                return NotFound(result.Value);
            }
            else if (result.Key == 0)
            {
                return BadRequest(result.Value);
            }

            return View();
        }

        [Authorize(Roles = "Project Manager")]
        public IActionResult PMEditProject(int projectId)
        {
            Project? project = _db.Projects.FirstOrDefault(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> PMEditProject(int projectId, string name, string desc, double budget, Priority priority, DateTime deadline)
        {
            Dictionary<int, string> resultDict = _projectHelper.Update(projectId, name, desc, budget, priority, deadline);
            KeyValuePair<int, string> result = resultDict.First();

            if (result.Key == -1)
            {
                return NotFound(result.Value);
            }
            else if (result.Key == 0)
            {
                return BadRequest(result.Value);
            }

            return RedirectToAction("PMDashboard");
        }
        
        public async Task<IActionResult> PMAddTask(int projectId)
        {         
            ViewBag.ProjectId = projectId;
            ViewBag.Project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

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
            ViewBag.ProjectId = projectId;
            ViewBag.Project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

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

        public async Task<IActionResult> PMAssignTask(int taskId)
        {
            List<ApplicationUser> developers = new List<ApplicationUser>();
            IdentityRole developerRole = await _roleManager.FindByNameAsync("Developer");
            foreach (var user in _db.Users.ToList())
            {
                if (await _mu.CheckForRole(user.Id, developerRole.Id))
                    developers.Add(user);
            }
            ViewBag.Developers = new SelectList(developers, "Id", "UserName");
            ViewBag.Task = _db.Tasks.First(t => t.Id == taskId);
            ViewBag.TaskId = taskId;
            ViewBag.ProjectId = _db.Tasks.First(t => t.Id == taskId).ProjectId;
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
            ViewBag.Task = _db.Tasks.First(t => t.Id == taskId);
            ViewBag.TaskId = taskId;
            ViewBag.ProjectId = _db.Tasks.First(t => t.Id == taskId).ProjectId;

            if (developers.First(d => d.Id == devId).Tasks.FirstOrDefault(t => t.Id == taskId) == null)
            {
                Dictionary<int, string> taskDict = await _th.Assign(devId, taskId);
                KeyValuePair<int, string> result = taskDict.First();
                if (result.Key == -1)
                    return NotFound(result.Value);
                else if (result.Key == 0)
                    return BadRequest(result.Value);
                ViewBag.Message = result.Value;
            }
            else
                ViewBag.Message = "Developer is already assigned to this task";
            
            return View();
        }

        public IActionResult PMUpdateTask(int taskId)
        {
            Models.Task? task = _db.Tasks.FirstOrDefault(t => t.Id == taskId);          

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        [HttpPost]
        public IActionResult PMUpdateTask(int id, string name, string desc, double budget, Priority priority, DateTime deadline)
        {
            Models.Task task = _db.Tasks.Include(p => p.Project).First(t => t.Id == id);
            Dictionary<int, string> taskDict = _th.Update(id, name, desc, budget, priority, deadline);
            KeyValuePair<int, string> result = taskDict.First();
            if (result.Key == -1)
                return NotFound(result.Value);
            else if (result.Key == 0)
                return BadRequest(result.Value);
            return RedirectToAction("PMProjectDetails", new { id = task.ProjectId, message = result.Value });
        }
    }
}

using finalprojectSD340.Data;
using finalprojectSD340.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace finalprojectSD340.Controllers
{
    public class PMController : Controller
    {

        public ApplicationDbContext _db;
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public PMController(ApplicationDbContext Db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = Db;
            _userManager = userManager;
            _roleManager = roleManager;
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





    }
}

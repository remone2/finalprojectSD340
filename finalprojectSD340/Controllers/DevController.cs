using finalprojectSD340.Data;
using finalprojectSD340.HelperClasses;
using finalprojectSD340.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace finalprojectSD340.Controllers
{
    public class DevController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly ProjectHelper _projectHelper;
        private readonly TaskHelper _taskHelper;
        public DevController(ApplicationDbContext db, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _projectHelper = new ProjectHelper(db, userManager);
            _taskHelper = new TaskHelper(db, userManager);
        }

        // GET: DevController
        public ActionResult Dashboard()
        {
            List<Models.Task> tasks = _db.Tasks.Where(p => p.DeveloperId == User.Identity.Name).ToList();
            return View(tasks);
        }

        [Authorize]
        public async Task<IActionResult> DeveloperTasks()
        {
            try
            {
                string userName = User.Identity.Name;
                ApplicationUser dev = await _userManager.FindByEmailAsync(userName);

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
                    task.CompleteDate = DateTime.Now;   // Kleis
                    await _taskHelper.CalculateTaskCost(taskId); // Kleis
                }

                await _db.SaveChangesAsync();

                var UserName = User.Identity.Name;
                ApplicationUser CurrentUser = await _userManager.FindByNameAsync(UserName);
                bool CheckDevRole = await _userManager.IsInRoleAsync(CurrentUser, "Developer");


                if (CheckDevRole != true)
                {
                    return RedirectToAction("PMProjectDetails", "PM", new { id = task.ProjectId });
                }
                else
                {
                    return RedirectToAction("DeveloperTasks");
                }

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
                    await CompleteTask(taskId);
                }

                return RedirectToAction("DeveloperTasks");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", ex.Message);
            }
        }

        [Authorize]
        public IActionResult TaskView(int taskId)
        {
            Models.Task task = _db.Tasks
                .Include(t => t.Developer)
                .Include(t => t.Project)
                .Include(t => t.Comments)
                .First(t => t.Id == taskId);

            return View(task);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MakeComment(int? taskId, string? comment, bool urgent)
        {
            try
            {
                string userName = User.Identity.Name;
                ApplicationUser commenter = await _userManager.FindByEmailAsync(userName);

                if (taskId == null)
                    return NotFound();

                Models.Task task = _db.Tasks.First(t => t.Id == taskId);

                if (string.IsNullOrWhiteSpace(comment))
                    return RedirectToAction("TaskView", task.Id);

                Comment newComment = new()
                {
                    Body = comment,
                    DateCreated = DateTime.Now,
                    Developer = commenter,
                    DeveloperId = commenter.Id,
                    Task = task,
                    TaskId = task.Id
                };

                if (urgent)
                {
                    newComment.IsUrgent = true;
                    //UrgentNotification();
                }

                commenter.Comments.Add(newComment);
                task.Comments.Add(newComment);
                _db.Comments.Add(newComment);

                await _userManager.UpdateAsync(commenter);
                await _db.SaveChangesAsync();

                return RedirectToAction("TaskView", new { taskId = task.Id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", ex.Message);
            }
        }

        //public async Task<IActionResult> UrgentNote(int? taskId, string? comment)
        //{
        //    try
        //    {
        //        string userName = User.Identity.Name;
        //        ApplicationUser commenter = await _userManager.FindByEmailAsync(userName);

        //        if (taskId == null)
        //            return NotFound();

        //        Models.Task task = _db.Tasks.First(t => t.Id == taskId);

        //        if (string.IsNullOrWhiteSpace(comment))
        //            return RedirectToAction("TaskView", taskId);

        //        Comment newComment = new()
        //        {
        //            Body = comment,
        //            DateCreated = DateTime.Now,
        //            Developer = commenter,
        //            DeveloperId = commenter.Id,
        //            IsUrgent = true,
        //            Task = task,
        //            TaskId = task.Id
        //        };

        //        //UrgentNotification();

        //        commenter.Comments.Add(newComment);
        //        task.Comments.Add(newComment);
        //        _db.Comments.Add(newComment);

        //        await _userManager.UpdateAsync(commenter);
        //        await _db.SaveChangesAsync();

        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        return RedirectToAction("Error", ex.Message);
        //    }
        //}

        // GET: DevController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DevController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DevController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DevController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DevController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DevController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DevController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}

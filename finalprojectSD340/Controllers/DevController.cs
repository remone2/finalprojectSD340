using finalprojectSD340.Data;
using finalprojectSD340.HelperClasses;
using finalprojectSD340.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace finalprojectSD340.Controllers
{

    public class DevController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly TaskHelper _taskHelper;
        private readonly NotificationHelper _notifHelper;
        
        public DevController(ApplicationDbContext db, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _taskHelper = new TaskHelper(db, userManager);
            _notifHelper = new NotificationHelper(_db, _userManager, _roleManager);
        }

        // GET: DevController
        [Authorize(Roles = "Developer, Admin")]
        public async Task<ActionResult> DeveloperDashboard()
        {
            try
            {
                string userName = User.Identity.Name;
                
                ApplicationUser user = _db.Users.First(u => u.UserName == userName);
                string deadlineNotifChecker = await _notifHelper.CheckDeadlineNotification(user.Id);
                if (deadlineNotifChecker != "Success")
                {
                    throw new Exception(deadlineNotifChecker);
                }

                ViewBag.Notifications = _db.Notifications.Where(n => !n.IsOpened && n.UserId == user.Id).ToList().Count;

                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { Message = ex.Message, Action = "DeveloperDashboard" });
            }
        }

        [Authorize(Roles = "Developer, Admin")]
        public async Task<IActionResult> DeveloperNotifications()
        {
            try
            {
                string userName = User.Identity.Name;
                ApplicationUser user = await _userManager.FindByEmailAsync(userName);
                List<Notification> unopenedNotifications = _db.Notifications
                    .Include(p => p.Project)
                    .Where(n => n.UserId == user.Id && !n.IsOpened)
                    .OrderByDescending(n => n.NotificationDate)
                    .ToList();

                List<Notification> openedNotifications = _db.Notifications
                    .Include(p => p.Project)
                    .Where(n => n.UserId == user.Id && n.IsOpened)
                    .OrderBy(n => n.NotificationDate)
                    .ToList();

                ViewBag.OpenedNotifications = openedNotifications;

                if (unopenedNotifications.Count == 0 && openedNotifications.Count == 0)
                {
                    ViewBag.Any = 1;
                }

                return View(unopenedNotifications);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { Message = ex.Message, Action = "DeveloperNotifications" });
            }
        }

        [Authorize(Roles = "Developer, Admin")]
        public async Task<IActionResult> DeveloperTasks()
        {
            try
            {
                string userName = User.Identity.Name;
                ApplicationUser dev = await _userManager.FindByEmailAsync(userName);

                List<Models.Task> devTasks = _db.Tasks
                    .Include(x => x.Project)
                    .Where(t => t.DeveloperId == dev.Id)
                    .ToList();

                return View(devTasks);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { Message = ex.Message, Action = "DeveloperTasks" });
            }
        }

        [Authorize(Roles = "Developer, Project Manager, Admin")]
        [HttpPost]
        public async Task<IActionResult> CompleteTask(int taskId)
        {
            try
            {
                string userName = User.Identity.Name;
                ApplicationUser user = await _userManager.FindByEmailAsync(userName);
                Models.Task task = _db.Tasks.First(t => t.Id == taskId);

                if (task == null)
                    return RedirectToAction("PMProjectDetails", "PM", new { id = task.ProjectId, message = "Task was not found." });

                if (task.DeveloperId == null)
                    return RedirectToAction("PMProjectDetails", "PM", new { id = task.ProjectId, message = "There is no developer assigned to this task." });

                if (task.IsCompleted)
                {
                    return RedirectToAction("PMProjectDetails", "PM", new { id = task.ProjectId, message = "Task has been completed." });
                }

                if (!task.IsCompleted)
                {
                    task.IsCompleted = true;
                    task.CompletionPercentage = 100;
                    task.CompleteDate = DateTime.Now;
                    await _taskHelper.CalculateTaskCost(taskId);
                }
                bool CheckDevRole = await _userManager.IsInRoleAsync(user, "Developer");

                if (CheckDevRole != true)
                {
                    await _db.SaveChangesAsync();
                    return RedirectToAction("PMProjectDetails", "PM", new { id = task.ProjectId });
                }

                if (!task.CompleteNotificationSent)
                {
                    string completeNotifChecker = await _notifHelper.CreateCompleteTaskNotification(user.Id, task.Id);
                    if (completeNotifChecker != "Success")
                    {
                        throw new Exception(completeNotifChecker);
                    }
                }

                await _db.SaveChangesAsync();

                return RedirectToAction("DeveloperTasks");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { Message = ex.Message, Action = "DeveloperTasks" });
            }
        }

        [Authorize(Roles = "Developer, Admin")]
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
                return RedirectToAction("Error", new { Message = ex.Message, Action = "DeveloperTasks" });
            }
        }

        public IActionResult TaskView(int taskId)
        {
            Models.Task task = _db.Tasks
                .Include(t => t.Developer)
                .Include(t => t.Project)
                .Include(t => t.Comments)
                .First(t => t.Id == taskId);

            return View(task);
        }

        [Authorize(Roles = "Developer, Admin")]
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
                    string urgentSender = await _notifHelper.UrgentNotification(commenter.Id, task.Id, newComment, NotificationType.Urgent);

                    if (urgentSender != "Success")
                    {
                        throw new Exception(urgentSender);
                    }
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
                return RedirectToAction("Error", new { Message = ex.Message, Action = "DeveloperTasks" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> OpenNotification(int id)
        {
            try
            {
                string openCheck = await _notifHelper.OpenNotification(id);

                if (openCheck != "Success")
                {
                    throw new Exception(openCheck);
                }

                return RedirectToAction("DeveloperNotifications");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { Message = ex.Message, Action = "DeveloperTasks" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            try
            {
                Notification notification = _db.Notifications.First(n => n.Id == notificationId);

                if (notification == null)
                {
                    throw new Exception("Notification Not Found");
                }

                _db.Notifications.Remove(notification);

                await _db.SaveChangesAsync();

                return RedirectToAction("DeveloperNotifications");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { Message = ex.Message, Action = "DeveloperNotifications" });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string Message, string Action)
        {
            ViewBag.Action = Action;
            ViewBag.Message = Message;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

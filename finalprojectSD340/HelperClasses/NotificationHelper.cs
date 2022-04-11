using finalprojectSD340.Data;
using finalprojectSD340.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace finalprojectSD340.HelperClasses
{
    public class NotificationHelper
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public NotificationHelper(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<string> OpenNotification(int id)
        {
            try
            {
                Notification notification = _db.Notifications.First(n => n.Id == id);

                if (notification == null)
                {
                    throw new Exception("Notification Not Found");
                }

                notification.IsOpened = true;

                await _db.SaveChangesAsync();

                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> CreateCompleteTaskNotification(string userId, int taskId)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(userId);
                Models.Task task = _db.Tasks.First(t => t.Id == taskId);
                Project project = _db.Projects.First(p => p.Id == task.ProjectId);
                ApplicationUser pm = _db.Users.First(u => u.Id == project.ProjectManagerId);

                Notification notification = new()
                {
                    Task = task,
                    TaskId = task.Id,
                    Project = project,
                    ProjectId = project.Id,
                    NotificationDate = DateTime.Now,
                    Type = NotificationType.Completed,
                    Title = "Task Complete",
                    User = pm,
                    UserId = pm.Id,
                    Body = $"{user.UserName} has completed task: {task.Name} for project: {project.Name}"
                };

                pm.Notifications.Add(notification);
                _db.Notifications.Add(notification);

                task.CompleteNotificationSent = true;

                await _userManager.UpdateAsync(user);
                await _db.SaveChangesAsync();

                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> CreateDeadlineNotifications(string userId, int taskId, NotificationType type)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(userId);
                Models.Task task = _db.Tasks.First(t => t.Id == taskId);
                Project taskProject = _db.Projects.First(p => p.Id == task.ProjectId);

                Notification notification = new()
                {
                    Task = task,
                    TaskId = task.Id,
                    Project = taskProject,
                    ProjectId = taskProject.Id,
                    NotificationDate = DateTime.Now,
                    User = user,
                    UserId = user.Id
                };

                if (type == NotificationType.DeadlineReminder)
                {
                    notification.Type = NotificationType.DeadlineReminder;
                    notification.Title = "Task Reminder";
                    notification.Body = $"Task: {task.Name} has one day left until it reaches it's deadline";
                    task.ReminderSent = true;
                }

                if (type == NotificationType.PastDeadline)
                {
                    notification.Title = "Deadline Reached";
                    notification.Body = $"Task: {task.Name}, is due, please complete or contact your project manager about time extension.";
                    notification.Type = NotificationType.PastDeadline;
                    task.DeadlineNotificationSent = true;
                }


                user.Notifications.Add(notification);
                _db.Notifications.Add(notification);
                taskProject.Notifications.Add(notification);
                task.Notifications.Add(notification);

                await _userManager.UpdateAsync(user);
                await _db.SaveChangesAsync();
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> UrgentNotification(string userId, int taskId, Comment comment, NotificationType type)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(userId);
                Models.Task task = _db.Tasks.First(t => t.Id == taskId);
                Project taskProject = _db.Projects.First(p => p.Id == task.ProjectId);
                ApplicationUser pm = _db.Users.First(u => u.Id == taskProject.ProjectManagerId);

                Notification notification = new()
                {
                    Task = task,
                    TaskId = task.Id,
                    Project = taskProject,
                    ProjectId = taskProject.Id,
                    NotificationDate = DateTime.Now,
                    User = pm,
                    UserId = pm.Id,
                    Type = type,
                    Comment = comment,
                    CommentId = comment.Id,
                    Title = "Urgent Notice!",
                    Body = comment.Body
                };

                pm.Notifications.Add(notification);
                _db.Notifications.Add(notification);

                await _userManager.UpdateAsync(user);
                await _db.SaveChangesAsync();

                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> CheckDeadlineNotification(string userId)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            List<Models.Task> userTasks = _db.Tasks.Where(t => t.DeveloperId == user.Id).ToList();

            foreach (var t in userTasks)
            {
                if (DateTime.Now.Day == t.Deadline.Day - 1 && !t.ReminderSent)
                {
                    string check = await CreateDeadlineNotifications(user.Id, t.Id, NotificationType.DeadlineReminder);
                    if (check != "Success")
                    {
                        return check;
                    }
                }

                if (DateTime.Now.Day == t.Deadline.Day && !t.DeadlineNotificationSent)
                {
                    string check = await CreateDeadlineNotifications(user.Id, t.Id, NotificationType.PastDeadline);
                    if (check != "Success")
                    {
                        return check;
                    }
                }
            }

            return "Success";
        }
    }
}

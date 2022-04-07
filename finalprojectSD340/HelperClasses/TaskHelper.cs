using finalprojectSD340.Models;
using finalprojectSD340.Data;
using Microsoft.AspNetCore.Identity;

namespace finalprojectSD340.HelperClasses
{
    public class TaskHelper : Helper
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskHelper(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _userManager = um;
        }

        public async Task<Dictionary<int, string>> Add(int projectId, string name, string desc, string? developerId, Priority priority, DateTime deadline)
        {
            if (name == null)
            {
                return new Dictionary<int, string>()
                {
                    { 0, "Insufficient data." },
                };
            }

            ApplicationUser? dev = null;
            Project project = _db.Projects.First(p => p.Id == projectId);

            if (developerId != null)
            {
                dev = await _userManager.FindByIdAsync(developerId);
            }

            try
            {
                Models.Task newTask = new Models.Task()
                {
                    Name = name,
                    Description = desc,
                    ProjectId = projectId,
                    Project = project,
                    DeveloperId = developerId,
                    Developer = dev,
                    Priority = priority,
                    Deadline = deadline,
                };

                await _db.AddAsync(newTask);
                if (dev != null)
                {
                    dev.Tasks.Add(newTask);
                }
                project.Tasks.Add(newTask);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new Dictionary<int, string>()
                {
                    { -1, ex.Message },
                };
            }

            return new Dictionary<int, string>()
            {
                { 1, "Task successfully created." },
            };
        }

        public override Dictionary<int, string> Delete(int id)
        {
            Models.Task? taskToDelete = _db.Tasks.FirstOrDefault(t => t.Id == id);

            if (taskToDelete != null)
            {
                try
                {
                    _db.Tasks.Remove(taskToDelete);
                    _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return new Dictionary<int, string>()
                    {
                        { -1, ex.Message },
                    };
                }
            }
            else
            {
                return new Dictionary<int, string>()
                {
                    { 0, "Task does not exist in the database." },
                };
            }

            return new Dictionary<int, string>() 
            {
                { 1, "Task successfully deleted." }, 
            };
        }

        public override Dictionary<int, string> UpdatePriority(int id, Priority newPriority)
        {
            Models.Task? taskToUpdate = _db.Tasks.FirstOrDefault(t => t.Id == id);

            if (taskToUpdate != null)
            {
                try
                {
                    taskToUpdate.Priority = newPriority;
                    _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return new Dictionary<int, string>()
                    {
                        { -1, ex.Message },
                    };
                }
            }
            else
            {
                return new Dictionary<int, string>()
                {
                    { 0, "Task does not exist in the database." },
                };
            }

            return new Dictionary<int, string>()
            {
                { 1, "Priority updated." },
            };
        }

        public override Dictionary<int, string> UpdateDeadline(int id, DateTime newDeadline)
        {
            Models.Task? taskToUpdate = _db.Tasks.FirstOrDefault(t => t.Id == id);

            if (taskToUpdate != null)
            {
                try
                {
                    taskToUpdate.Deadline = newDeadline;
                    _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    return new Dictionary<int, string>()
                    {
                        { -1, ex.Message },
                    };
                }
            }
            else
            {
                return new Dictionary<int, string>()
                {
                    { 0, "Task does not exist in the database." },
                };
            }
            return new Dictionary<int, string>()
            {
                { 1, "Deadline updated." },
            };
        }

        public async Task<Dictionary<int, string>> Assign(string devIdToAssign, int taskId)
        {
            Models.Task? taskToAssign = _db.Tasks.FirstOrDefault(t => t.Id == taskId);
            ApplicationUser? dev = await _userManager.FindByIdAsync(devIdToAssign);

            if (taskToAssign == null)
                return new Dictionary<int, string>()
                {
                    { 0, "Task does not exist in the database." },
                };

            if (dev == null)
                return new Dictionary<int, string>()
                {
                    { 0, "Developer does not exist in the database." },
                };

            try
            {
                taskToAssign.DeveloperId = dev.Id;
                taskToAssign.Developer = dev;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new Dictionary<int, string>()
                {
                    { -1, ex.Message },
                };
            }

            return new Dictionary<int, string>()
            {
                { 1, "Developer has been assigned." },
            };
        }
    }
}

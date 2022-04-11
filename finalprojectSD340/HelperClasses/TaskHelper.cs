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
                    StartDate = DateTime.Now, // Kleis added
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

        public override Dictionary<int, string> Update(int id, string name, string desc, double budget, Priority priority, DateTime deadline)
        {
            Models.Task? taskToUpdate = _db.Tasks.FirstOrDefault(t => t.Id == id);

            if (taskToUpdate == null)
            {
                return new Dictionary<int, string>()
                {
                    { 0, "Task does not exist in the database." },
                };
            }

            try
            {
                if (taskToUpdate.Name != name)
                {
                    taskToUpdate.Name = name;
                }

                if (taskToUpdate.Description != desc)
                {
                    taskToUpdate.Description = desc;
                }

                if (taskToUpdate.Priority != priority)
                {
                    taskToUpdate.Priority = priority;
                }

                if (taskToUpdate.Deadline != deadline)
                {
                    taskToUpdate.Deadline = deadline;
                }

                _db.SaveChanges();
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
                { 1, "Task successfully updated." },
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
                taskToAssign.StartDate = DateTime.Now; // Kleis added
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




        public async Task<string>  CalculateTaskCost(int id)
        {
            Models.Task CurrentTask = _db.Tasks.First(t => t.Id == id);
            ApplicationUser Dev = await _userManager.FindByIdAsync(CurrentTask.DeveloperId);

            double Salary = Dev.Salary;
            var TimeSpan = CurrentTask.CompleteDate - CurrentTask.StartDate;
            int DayCount = TimeSpan.Days;

            CurrentTask.TaskCost = DayCount * Salary;
            await _db.SaveChangesAsync();
            return "Cost calculated";
        }

        //public string CompleteTask(int id)
        //{
        //    Models.Task CurrentTask = _db.Tasks.First(t => t.Id == id);

        //    if (CurrentTask != null)
        //    {
        //        return "Could not identify the task.";
        //    }

        //    try
        //    {
        //        CurrentTask.CompleteDate = DateTime.Now;
        //        CurrentTask.CompletionPercentage = 100;
        //        CurrentTask.IsCompleted = true;

        //        CalculateTaskCost(id);
        //        //_db.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message;
        //    }

        //    return "Task completed!";
        //}
    }
}
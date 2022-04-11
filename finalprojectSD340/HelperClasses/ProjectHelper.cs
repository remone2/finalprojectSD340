using finalprojectSD340.Models;
using finalprojectSD340.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace finalprojectSD340.HelperClasses
{
    public class ProjectHelper : Helper
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectHelper(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _userManager = um;
        }

        public async Task<Dictionary<int, string>> Add(string name, string desc, double budget, string projectManagerId, Priority priority, DateTime deadline)
        {
            if (name == null || projectManagerId == null)
            {
                return new Dictionary<int, string>()
                {
                    { 0, "Insufficient data." },
                };
            }

            ApplicationUser? user = await _userManager.FindByIdAsync(projectManagerId);

            if (user == null)
            {
                return new Dictionary<int, string>()
                {
                    { 0, "Manager does not exist in the database." },
                };
            }

            try
            {
                Project newProject = new Project()
                {
                    Name = name,
                    Description = desc,
                    Budget = budget,
                    ProjectManager = user,
                    ProjectManagerId = projectManagerId,
                    Priority = priority,
                    Deadline = deadline,
                };

                await _db.Projects.AddAsync(newProject);
                user.Projects.Add(newProject);
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
                { 1, "Project added successfully." },
            };
        }

        public override Dictionary<int, string> Delete(int id)
        {
            Project? projectToDelete = _db.Projects.Include(p => p.Notifications)
                .Include(p => p.Tasks).ThenInclude(t => t.Comments)
                .Include(p => p.Tasks).ThenInclude(t => t.Notifications).FirstOrDefault(p => p.Id == id);

            if (projectToDelete != null)
            {
                try
                {
                    List<Models.Task> tasks = projectToDelete.Tasks.ToList();
                    List<Comment> commentsToDelete = new List<Comment>();
                    foreach (var t in tasks)
                    {
                        foreach (var c in t.Comments)
                        {
                            commentsToDelete.Add(c);
                        }                        
                    }
                    
                    foreach (var c in commentsToDelete)
                    {               
                        _db.Comments.Remove(c);
                    }

                    List<Notification> notifsToDelete = new List<Notification>();
                    foreach (var t in tasks)
                    {
                        foreach (var n in t.Notifications)
                        {
                            notifsToDelete.Add(n);
                        }
                    }

                    foreach (var n in notifsToDelete)
                    {
                        _db.Notifications.Remove(n);
                    }

                    _db.Projects.Remove(projectToDelete);
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
                    { 0, "Project does not exist in the database." },
                };
            }

            return new Dictionary<int, string>()
            {
                { 1, "Project successfully deleted." },
            };
        }

        public override Dictionary<int, string> Update(int id, string name, string desc, double budget, Priority priority, DateTime deadline)
        {
            Project? projectToUpdate = _db.Projects.FirstOrDefault(p => p.Id == id);

            if (projectToUpdate == null)
            {
                return new Dictionary<int, string>()
                {
                    { 0, "Project does not exist in the database." },
                };
            }

            try
            {
                if (projectToUpdate.Name != name)
                {
                    projectToUpdate.Name = name;
                }

                if (projectToUpdate.Description != desc)
                {
                    projectToUpdate.Description = desc;
                }

                if (projectToUpdate.Budget != budget)
                {
                    projectToUpdate.Budget = budget;
                }

                if (projectToUpdate.Priority != priority)
                {
                    projectToUpdate.Priority = priority;
                }
                
                if (projectToUpdate.Deadline != deadline)
                {
                    projectToUpdate.Deadline = deadline;
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
                { 1, "Project successfully updated." },
            };
        }



        public async Task<string> CalculateProjectCost(int id)
        {
            Project CurrentProject = _db.Projects.Where(p => p.Id == id).First();

            ApplicationUser PM = await _userManager.FindByIdAsync(CurrentProject.ProjectManagerId);
            double Salary = PM.Salary;
            double TotalTaskCost = CurrentProject.Tasks.Select(t => t.TaskCost).Sum();
            CurrentProject.ProjectCost = Salary + TotalTaskCost;
            await _db.SaveChangesAsync();
            return "Cost calculated";
        }

        public async Task<string> CompleteProject(int id)
        {
            Project CurrentProject = _db.Projects.First(p => p.Id == id);

            if (CurrentProject == null)
            {
                return "Could not identify the project.";
            }

            if (CurrentProject.Tasks.Any(t => t.IsCompleted == false))
            {
                return "Please ensure all tasks within this project are set to complete.";
            }

            try
            {
                await CalculateProjectCost(id);
                CurrentProject.IsComplete = true;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "Project completed!";
        }
    }
}
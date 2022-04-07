using finalprojectSD340.Models;
using finalprojectSD340.Data;
using Microsoft.AspNetCore.Identity;

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
            Project? projectToDelete = _db.Projects.FirstOrDefault(p => p.Id == id);

            if (projectToDelete != null)
            {
                try
                {
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

        public override Dictionary<int, string> UpdatePriority(int id, Priority newPriority)
        {
            Project? projectToUpdate = _db.Projects.FirstOrDefault(p => p.Id == id);

            if (projectToUpdate != null)
            {
                try
                {
                    projectToUpdate.Priority = newPriority;
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
                { 1, "Priority updated." },
            };
        }

        public override Dictionary<int, string> UpdateDeadline(int id, DateTime newDeadline)
        {
            Project? projectToUpdate = _db.Projects.FirstOrDefault(p => p.Id == id);

            if (projectToUpdate != null)
            {
                try
                {
                    projectToUpdate.Deadline = newDeadline;
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
                { 1, "Deadline updated." },
            };
        }
    }
}
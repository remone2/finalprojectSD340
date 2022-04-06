using finalprojectSD340.Models;
using finalprojectSD340.Data;

namespace finalprojectSD340.HelperClasses
{
    public class ProjectHelper : Helper
    {
        private readonly ApplicationDbContext _db;

        public ProjectHelper(ApplicationDbContext db)
        {
            _db = db;
        }
        public override string Add(int projectId)
        {
            return "";
        }

        public override string Delete(int id)
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
                    return ex.Message;
                }
            }
            else
            {
                return "Project does not exist in the database.";
            }

            return "Project successfully deleted.";
        }

        public override string UpdatePriority(int id, Priority newPriority) 
        {
            return "Priority updated.";
        }

        public override string UpdateDeadline(int id, DateTime newDeadline)
        {
            return "Deadline updated.";
        }
    }
}

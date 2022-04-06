using finalprojectSD340.Models;
using finalprojectSD340.Data;

namespace finalprojectSD340.HelperClasses
{
    public abstract class Helper
    {
        public abstract string Add(int projectId);

        public abstract string Delete(int id);

        public abstract string UpdatePriority(int id, Priority newPriority);

        public abstract string UpdateDeadline(int id, DateTime newDeadline);
    }
}

using finalprojectSD340.Models;
using finalprojectSD340.Data;

namespace finalprojectSD340.HelperClasses
{
    public abstract class Helper
    {

        public abstract Dictionary<int, string> Delete(int id);

        public abstract Dictionary<int, string> UpdatePriority(int id, Priority newPriority);

        public abstract Dictionary<int, string> UpdateDeadline(int id, DateTime newDeadline);
    }
}

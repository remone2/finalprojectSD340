using finalprojectSD340.Models;
using finalprojectSD340.Data;

namespace finalprojectSD340.HelperClasses
{
    public abstract class Helper
    {

        public abstract Dictionary<int, string> Delete(int id);

        public abstract Dictionary<int, string> Update(int id, string name, string desc, double budget, Priority priority, DateTime deadline);
    }
}

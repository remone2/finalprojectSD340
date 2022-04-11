namespace finalprojectSD340.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsComplete { get; set; } = false;
        public double Budget { get; set; }
        public double ProjectCost { get; set; }
        public ICollection<Task> Tasks { get; set; }
        public string ProjectManagerId { get; set; }
        public ApplicationUser ProjectManager { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public Priority Priority { get; set; }
        public DateTime Deadline { get; set; }
        public Project()
        {
            Tasks = new HashSet<Task>();
            Notifications = new HashSet<Notification>();
        }      
    }
    public enum Priority
    {
        High,
        Moderate,
        Low
    };
}

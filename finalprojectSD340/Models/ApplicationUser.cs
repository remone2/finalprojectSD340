using Microsoft.AspNetCore.Identity;

namespace finalprojectSD340.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Task> Tasks { get; set; }
        public ICollection<Project> Projects { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public double Salary { get; set; }
        public ApplicationUser()
        {
            Comments = new HashSet<Comment>();
            Tasks = new HashSet<Task>();
            Projects = new HashSet<Project>();
            Notifications = new HashSet<Notification>();
        }
    }
}

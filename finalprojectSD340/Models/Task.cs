using System.ComponentModel.DataAnnotations;

namespace finalprojectSD340.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; } = false;
        [Display(Name = "Completion Percentage")]
        public int CompletionPercentage { get; set; } = 0;
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public string? DeveloperId { get; set; }
        public ApplicationUser? Developer { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public Priority Priority { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime CompleteDate { get; set; }
        public double TaskCost { get; set; }
        public bool ReminderSent { get; set; } = false;
        public bool DeadlineNotificationSent { get; set; } = false;
        public bool CompleteNotificationSent { get; set; } = false;

        public Task()
        {
            Comments = new HashSet<Comment>();
            Notifications = new HashSet<Notification>();
        }

    }
}

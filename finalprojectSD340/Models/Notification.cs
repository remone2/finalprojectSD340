using System.ComponentModel.DataAnnotations.Schema;

namespace finalprojectSD340.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public NotificationType Type { get; set; }
        public bool IsOpened { get; set; } = false;
        public DateTime NotificationDate { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public int? TaskId { get; set; }
        public Task? Task { get; set; }
        public int? CommentId { get; set; }
        public Comment? Comment { get; set; }
        public ApplicationUser? User { get; set; }
        public string? UserId { get; set; }
    }
    public enum NotificationType
    {
        DeadlineReminder,
        PastDeadline,
        Urgent,
        Completed
    };
}

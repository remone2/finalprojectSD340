using System.ComponentModel.DataAnnotations.Schema;

namespace finalprojectSD340.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsUrgent { get; set; }
        public string DeveloperId { get; set; }
        public ApplicationUser Developer { get; set; }
        public int? TaskId { get; set; }
        [ForeignKey("TaskId")]
        public Task? Task { get; set; }
        public Notification? Notification { get; set; }
    }
}

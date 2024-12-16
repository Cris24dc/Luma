using System.ComponentModel.DataAnnotations;

namespace Luma.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Comments should have text")]
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public string? UserId { get; set; }
        public virtual User? User { get; set; }
        public int TaskId { get; set; }
        public virtual Task? Task { get; set; }
    }
}

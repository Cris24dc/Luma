using System.ComponentModel.DataAnnotations;

namespace Luma.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        public string? Project_Name { get; set; }
        public string? Organizer { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Task>? Tasks { get; set; }
    }
}
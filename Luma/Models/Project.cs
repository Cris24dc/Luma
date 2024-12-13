using System.ComponentModel.DataAnnotations;

namespace Luma.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Project name is invalid")]
        [StringLength(20, ErrorMessage = "Name cannot be longer than 20 characters")]
        [MinLength(5, ErrorMessage = "Name needs to be at least 5 characters")]
        public string? Project_Name { get; set; }
        public string? Organizer { get; set; }
        public virtual ICollection<User>? Users { get; set; }
        public virtual ICollection<Task>? Tasks { get; set; }
    }
}

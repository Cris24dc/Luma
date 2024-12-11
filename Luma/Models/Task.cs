using System.ComponentModel.DataAnnotations;

namespace Luma.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }
        public virtual ICollection<User>? Users { get; set; }

        // cheie externa (FK) - un task apartine unui proiect
        public int ProjectId { get; set; }
        // proprietatea virtuala - un task apartine unui proiect
        public virtual Project? Project { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public DateTime Start_Date { get; set; }

        public DateTime End_Date { get; set; }

        // public string Media { get; set; }
    }
}
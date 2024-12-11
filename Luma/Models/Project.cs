using System.ComponentModel.DataAnnotations;

namespace Luma.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        public string Project_Name { get; set; }


        public string UserId { get; set; }
        public virtual User User { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Luma.Models
{
    public class Projects
    {
        [Key]
        public int Id { get; set; }
        public string Project_Name { get; set; }
    }
}

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

        // PASUL 6: useri si roluri 
        // cheie externa (FK) - un comentariu este postat de catre un user
        public string? UserId { get; set; }

        // PASUL 6: useri si roluri 
        // proprietatea virtuala - un comentariu este postat de catre un user
        public virtual User? User { get; set; }

        // cheie externa (FK) - un comentariu apartine unui task
        public int TaskId { get; set; }
        // proprietatea virtuala - un comentariu apartine unui task
        public virtual Task? Task { get; set; }
    }
}

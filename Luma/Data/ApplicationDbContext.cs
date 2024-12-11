using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Luma.Models;
using ProjectModel = Luma.Models.Project;
using TaskModel = Luma.Models.Task;

namespace Luma.Data
{
    //Roluri si Useri   
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<ProjectModel> Projects { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}

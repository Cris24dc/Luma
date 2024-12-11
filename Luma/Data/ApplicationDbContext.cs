using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Luma.Models;
using Microsoft.CodeAnalysis;

namespace Luma.Data
{
    //Roluri si Useri   
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }



        public DbSet<Project> Projects { get; set; }

        public DbSet<Comment> Comments { get; set; }
    }
}

using Luma.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Luma.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService
            <DbContextOptions<ApplicationDbContext>>()))

            {
                if (context.Roles.Any())
                {
                    return;
                }

                context.Roles.AddRange(
                new IdentityRole { Id = "7161a6d9-efd8-4f60-ae91-e85a3bd21270", Name = "Admin", NormalizedName = "Admin".ToUpper() },
                new IdentityRole { Id = "7161a6d9-efd8-4f60-ae91-e85a3bd21271", Name = "Member", NormalizedName = "Member".ToUpper() }
                );

                var hasher = new PasswordHasher<User>();

                context.Users.AddRange(
                new User
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                    UserName = "admin@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    Email = "admin@test.com",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "Admin1!")
                },

                new User
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                    UserName = "member@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "MEMBER@TEST.COM",
                    Email = "member@test.com",
                    NormalizedUserName = "MEMBER@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "Member1!")
                });

                context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {
                    RoleId = "7161a6d9-efd8-4f60-ae91-e85a3bd21270",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "7161a6d9-efd8-4f60-ae91-e85a3bd21271",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                });

                context.SaveChanges();
            }
        }
    }
}

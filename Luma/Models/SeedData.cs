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
                // Verificam daca in baza de date exista cel putin un rol
                // insemnand ca a fost rulat codul
                // De aceea facem return pentru a nu insera rolurile inca o data
                // Acesta metoda trebuie sa se execute o singura data
                if (context.Roles.Any())
                {
                    return; // baza de date contine deja roluri
                }

                // CREAREA ROLURILOR IN BD
                // daca nu contine roluri, acestea se vor crea

                context.Roles.AddRange(
                new IdentityRole { Id = "7161a6d9-efd8-4f60-ae91-e85a3bd21270", Name = "Admin", NormalizedName = "Admin".ToUpper() },
                new IdentityRole { Id = "7161a6d9-efd8-4f60-ae91-e85a3bd21271", Name = "Member", NormalizedName = "Member".ToUpper() }
                );

                // o noua instanta pe care o vom utiliza pentru crearea parolelor utilizatorilor
                // parolele sunt de tip hash

                var hasher = new PasswordHasher<User>();

                // CREAREA USERILOR IN BD
                // Se creeaza cate un user pentru fiecare rol
                context.Users.AddRange(
                new User
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb0", // primary key
                    UserName = "admin@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    Email = "admin@test.com",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "Admin1!")
                },

                new User
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb1", // primary key
                    UserName = "member@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "MEMBER@TEST.COM",
                    Email = "member@test.com",
                    NormalizedUserName = "MEMBER@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "Member1!")
                });


                // ASOCIEREA USER-ROLE
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

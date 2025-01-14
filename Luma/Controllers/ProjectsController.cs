using Luma.Data;
using Luma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using ProjectModel = Luma.Models.Project;

namespace Luma.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        // users and roles:
        private readonly ApplicationDbContext db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ProjectsController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Index Action:
        [Authorize(Roles = "Member,Admin")]
        public IActionResult Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            ViewBag.IsAdmin = isAdmin;

            // Dacă utilizatorul este admin, returnează toate proiectele
            if (isAdmin)
            {
                var projects = db.Projects.Include(p => p.Users).ToList();
                ViewBag.Projects = projects;

                var projectViewModels = projects.Select(project => new
                {
                    Project = project,
                    OrganizerUsername = project.Users.FirstOrDefault(u => u.Id == project.Organizer)?.UserName
                }).ToList();

                ViewBag.ProjectViewModels = projectViewModels;
                return View();
            }
            else
            {
                // Dacă utilizatorul este doar Member, returnează proiectele în care este implicat
                var projects = db.Projects.Include(p => p.Users)
                                          .Where(p => p.Users.Any(u => u.Id == currentUserId))
                                          .ToList();

                ViewBag.Projects = projects;

                var projectViewModels = projects.Select(project => new
                {
                    Project = project,
                    OrganizerUsername = project.Users.FirstOrDefault(u => u.Id == project.Organizer)?.UserName
                }).ToList();

                ViewBag.ProjectViewModels = projectViewModels;
                return View();
            }
        }

        // GET: New Action
        [Authorize(Roles = "Member, Admin")]
        public IActionResult New()
        {
            return View();
        }

        // POST: New Action
        [HttpPost]
        [Authorize(Roles = "Member, Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult New(ProjectModel project)
        {
            if (ModelState.IsValid)
            {
                string userId = _userManager.GetUserId(User);
                project.Organizer = userId;

                var user = db.Users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    project.Users = new List<User> { user };
                    db.Projects.Add(project);
                    db.SaveChanges();
                    TempData["message"] = "Project was created successfully.";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "User not found.";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }

            return View(project);
        }

        // GET: Edit Action
        [Authorize(Roles = "Member, Admin")]
        public IActionResult Edit(int id)
        {
            var project = db.Projects.FirstOrDefault(p => p.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            // Check if the current user is the organizer or admin
            if (project.Organizer == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(project);
            }

            TempData["message"] = "You do not have permission to edit this project.";
            TempData["messageType"] = "alert-danger";
            return RedirectToAction("Index");
        }

        // POST: Edit Action
        [HttpPost]
        [Authorize(Roles = "Member, Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ProjectModel editedProject)
        {
            var project = db.Projects.FirstOrDefault(p => p.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            if (project.Organizer == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                project.Project_Name = editedProject.Project_Name;

                db.SaveChanges();

                TempData["message"] = "Project was modified successfully.";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            TempData["message"] = "You do not have permission to edit this project.";
            TempData["messageType"] = "alert-danger";
            return RedirectToAction("Index");
        }

        // POST: Delete Action
        [HttpPost]
        [Authorize(Roles = "Member, Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var project = db.Projects.FirstOrDefault(p => p.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            if (project.Organizer == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Projects.Remove(project);
                db.SaveChanges();

                TempData["message"] = "Project was deleted successfully.";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            TempData["message"] = "You do not have permission to delete this project.";
            TempData["messageType"] = "alert-danger";
            return RedirectToAction("Index");
        }

        // GET: AllUsers
        [Authorize(Roles = "Member, Admin")]
        public IActionResult AllUsers(int projectId)
        {
            // Fetch the admin role ID
            var adminRoleId = db.Roles.FirstOrDefault(r => r.Name == "Admin")?.Id;
            if (adminRoleId == null)
            {
                return NotFound("Admin role doesn't exist.");
            }

            // Get the current user ID
            var currentUserId = _userManager.GetUserId(User);

            // Get the list of users who are not Admins and not the current user
            var adminUserIds = db.UserRoles
                                 .Where(ur => ur.RoleId == adminRoleId)
                                 .Select(ur => ur.UserId)
                                 .ToList();

            var users = db.Users
                          .Where(u => !adminUserIds.Contains(u.Id) && u.Id != currentUserId)
                          .ToList();

            // Fetch the project by ID to check the users already associated with it
            var project = db.Projects.Include(p => p.Users).FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return NotFound("Project not found.");
            }

            // Prepare the data that includes whether each user is assigned to the project
            var usersWithProjectInfo = users.Select(user => new
            {
                User = user,
                IsAssignedToProject = project.Users.Contains(user)
            }).ToList();

            // Pass the users, usersWithProjectInfo, and projectId to the view
            ViewBag.Users = users;
            ViewBag.UsersWithProjectInfo = usersWithProjectInfo;
            ViewBag.ProjectId = projectId;

            return View();
        }

        // POST: Add User to Project
        [HttpPost]
        [Authorize(Roles = "Member, Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult AddUserToProject(int projectId, string userId)
        {
            var project = db.Projects.Include(p => p.Users).FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return NotFound("Project not found.");
            }

            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Add the user to the project
            if (!project.Users.Contains(user))
            {
                project.Users.Add(user);
                db.SaveChanges();
            }

            return RedirectToAction("AllUsers", new { projectId = projectId });
        }

        // POST: Remove User from Project
        [HttpPost]
        [Authorize(Roles = "Member, Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveUserFromProject(int projectId, string userId)
        {
            var project = db.Projects.Include(p => p.Users).FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return NotFound("Project not found.");
            }

            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Remove the user from the project
            if (project.Users.Contains(user))
            {
                project.Users.Remove(user);
                db.SaveChanges();
            }

            return RedirectToAction("AllUsers", new { projectId = projectId });
        }

        // GET: AdminUsers
        [Authorize(Roles = "Admin")]
        public IActionResult AdminUsers()
        {
            var adminRoleId = db.Roles.FirstOrDefault(r => r.Name == "Admin")?.Id;
            if (adminRoleId == null)
            {
                return NotFound("Admin role doesn't exist.");
            }

            var currentUserId = _userManager.GetUserId(User);

            var users = db.Users
                          .Where(u => u.Id != currentUserId)
                          .Select(u => new
                          {
                              User = u,
                              IsAdmin = db.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleId)
                          })
                          .ToList();

            ViewBag.UsersWithAdminInfo = users;
            return View();
        }

        // POST: Add Admin Role
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult AddAdmin(string userId)
        {
            var adminRole = db.Roles.FirstOrDefault(r => r.Name == "Admin");
            if (adminRole == null)
            {
                return NotFound("Admin role doesn't exist.");
            }

            var userRole = new IdentityUserRole<string>
            {
                UserId = userId,
                RoleId = adminRole.Id
            };

            db.UserRoles.Add(userRole);
            db.SaveChanges();

            return RedirectToAction("AdminUsers");
        }

        // POST: Remove Admin Role
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveAdmin(string userId)
        {
            var adminRole = db.Roles.FirstOrDefault(r => r.Name == "Admin");
            if (adminRole == null)
            {
                return NotFound("Admin role doesn't exist.");
            }

            var userRole = db.UserRoles.FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == adminRole.Id);
            if (userRole != null)
            {
                db.UserRoles.Remove(userRole);
                db.SaveChanges();
            }

            return RedirectToAction("AdminUsers");
        }

        // POST: Delete User
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(string userId)
        {
            var user = db.Users
                .Include(u => u.Comments)
                .Include(u => u.Projects)
                .Include(u => u.Tasks)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.Comments != null)
            {
                db.Comments.RemoveRange(user.Comments);
            }

            var projects = db.Projects.Where(p => p.Users.Any(u => u.Id == userId)).ToList();
            foreach (var project in projects)
            {
                project.Users.Remove(user);
            }

            var tasks = db.Tasks.Where(t => t.Users.Any(u => u.Id == userId)).ToList();
            foreach (var task in tasks)
            {
                task.Users.Remove(user);
            }

            db.Users.Remove(user);

            db.SaveChanges();

            return RedirectToAction("AdminUsers");
        }


        // See if it has CRUD rights
        private void SetAccessRights(ProjectModel project)
        {
            ViewBag.ShowButtons = false;

            var currentUserId = _userManager.GetUserId(User);
            if (project.Organizer == currentUserId || User.IsInRole("Admin"))
            {
                ViewBag.ShowButtons = true;
            }
        }
    }
}

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
            if (User.IsInRole("Member"))
            {
                var projects = from project in db.Projects.Include("Users")
                           .Where(a => a.Users.Any(b => b.Id == _userManager.GetUserId(User)))
                               select project;

                ViewBag.Projects = projects;

                var projectViewModels = projects.Select(project => new
                {
                    Project = project,
                    OrganizerUsername = project.Users.FirstOrDefault(u => u.Id == project.Organizer).UserName
                }).ToList();

                ViewBag.ProjectViewModels = projectViewModels;


                return View();

            }
            else
            if (User.IsInRole("Admin"))
            {
                var projects = from project in db.Projects.Include("Users")
                               select project;
                ViewBag.Projects = projects;

                var projectViewModels = projects.Select(project => new
                {
                    Project = project,
                    OrganizerUsername = project.Users.FirstOrDefault(u => u.Id == project.Organizer).UserName
                }).ToList();

                ViewBag.ProjectViewModels = projectViewModels;

                return View();
            }

            else
            {
                TempData["message"] = "Nu aveti drepturi asupra colectiei";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Projects");
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

using Luma.Data;
using Luma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TaskModel = Luma.Models.Task;
using ProjectModel = Luma.Models.Project;

namespace Luma.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        // users and roles:
        private readonly ApplicationDbContext db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public TasksController(
        ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Index Action:
        [Authorize(Roles = "Member, Admin")]
        public IActionResult Index(int projectId)
        {
            var project = db.Projects
                .Include(p => p.Users)
                .Include(p => p.Tasks)
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            // Get current user id
            var currentUserId = _userManager.GetUserId(User);

            // Verify if user is admin
            if (!User.IsInRole("Admin"))
            {
                // Verify if user is in the current project
                if (!project.Users.Any(u => u.Id == currentUserId))
                {
                    // Deny the access
                    return Forbid();
                }
            }

            // Get the number of project members (check if Users is null)
            var numberOfMembers = project.Users?.Count() ?? 0;
            ViewBag.NumberOfMembers = numberOfMembers;

            // Organizer user
            var organizer = _userManager.Users.FirstOrDefault(u => u.Id == project.Organizer);

            // Select organizer username
            ViewBag.OrganizerName = organizer?.UserName ?? "Unknown Organizer";

            // CRUD for organizer
            SetAccessRights(project);

            // Tasks by Status
            var tasks = project.Tasks;
            ViewBag.ToDoTasks = tasks.Where(t => t.Status == "To do").ToList();
            ViewBag.InProgressTasks = tasks.Where(t => t.Status == "In progress").ToList();
            ViewBag.DoneTasks = tasks.Where(t => t.Status == "Done").ToList();

            return View(project);
        }


        // GET: Tasks/New
        [Authorize]
        public IActionResult New(int projectId, string status)
        {
            var project = db.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return NotFound();
            }

            var task = new TaskModel
            {
                ProjectId = projectId,
                Status = status,
                Start_Date = DateTime.Now,
                End_Date = DateTime.Now.AddDays(1)
            };

            return View(task);
        }

        // POST: Tasks/New
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult New(TaskModel task)
        {
            if (ModelState.IsValid)
            {
                db.Tasks.Add(task);
                db.SaveChanges();
                return RedirectToAction("Index", "Tasks", new { projectId = task.ProjectId });
            }

            return View(task);
        }

        // GET: Show Action
        [Authorize]
        public IActionResult Show(int id)
        {
            // Include the Users in the query
            var task = db.Tasks
                         .Include(t => t.Project)
                         .Include(t => t.Users) // Ensure Users are loaded
                         .FirstOrDefault(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

            // Verify if is Organizer or Admin for CRUD
            ViewBag.ShowButtons = task.Project.Organizer == currentUserId || User.IsInRole("Admin");

            // Fetch the admin role ID
            var adminRoleId = db.Roles.FirstOrDefault(r => r.Name == "Admin")?.Id;
            if (adminRoleId == null)
            {
                return NotFound("Admin role doesn't exist.");
            }

            // Get the list of users who are not Admins and not the current user
            var adminUserIds = db.UserRoles
                                 .Where(ur => ur.RoleId == adminRoleId)
                                 .Select(ur => ur.UserId)
                                 .ToList();

            var users = db.Users
                          .Where(u => !adminUserIds.Contains(u.Id) && u.Id != currentUserId)
                          .ToList();

            // Prepare the data that includes whether each user is assigned to the task
            var usersWithinTask = users.Select(user => new
            {
                User = user,
                IsAssignedToTask = task.Users.Contains(user) // Ensure this won't throw an exception
            }).ToList();

            // Pass the users, usersWithProjectInfo, and projectId to the view
            ViewBag.Users = users;
            ViewBag.UsersWithinTask = usersWithinTask;
            ViewBag.ProjectId = task.Id;

            var comments = db.Comments.Where(a => a.TaskId == id);
            ViewBag.Comments = comments;

            return View(task);
        }


        // GET: Edit Action
        [Authorize]
        public IActionResult Edit(int id)
        {
            var task = db.Tasks.Find(id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Edit Action
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TaskModel task)
        {
            if (ModelState.IsValid)
            {
                db.Tasks.Update(task);
                db.SaveChanges();
                // Redirect to the project page
                return RedirectToAction("Index", "Tasks", new { projectId = task.ProjectId });
            }

            return View(task);
        }

        // POST: Delete Action
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var task = db.Tasks.Find(id);
            if (task == null)
            {
                return NotFound();
            }

            // Get the projectId of the task
            var projectId = task.ProjectId;
            db.Tasks.Remove(task);
            db.SaveChanges();

            // Redirect to the project page
            return RedirectToAction("Index", "Tasks", new { projectId = projectId });
        }

        // GET: AllUsers
        [Authorize(Roles = "Member, Admin")]
        public IActionResult AllUsers(int projectId)
        {
            return View();
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

        // POST: UpdateStatus
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int id, string status)
        {
            var task = db.Tasks.Find(id);
            if (task == null)
            {
                return NotFound();
            }

            task.Status = status;
            db.Tasks.Update(task);
            db.SaveChanges();

            return RedirectToAction("Show", new { id = task.Id });
        }

    }
}

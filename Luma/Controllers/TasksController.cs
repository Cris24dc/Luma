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

        // GET: Index Action:
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


        // GET: New Action
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

        // POST: New Action
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
            var task = db.Tasks
                         .Include(t => t.Project)
                         .Include(t => t.Users)
                         .Include(t => t.Comments)
                             .ThenInclude(c => c.User)
                         .FirstOrDefault(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

            // Verify if is Organizer or Admin for CRUD
            ViewBag.ShowButtons = task.Project.Organizer == currentUserId || User.IsInRole("Admin");

            // Get all admins role ID
            var adminRoleId = db.Roles.FirstOrDefault(r => r.Name == "Admin")?.Id;
            if (adminRoleId == null)
            {
                return NotFound("Admin role doesn't exist.");
            }

            // Get the users who are not Admins and not the current user
            var adminUserIds = db.UserRoles
                                 .Where(ur => ur.RoleId == adminRoleId)
                                 .Select(ur => ur.UserId)
                                 .ToList();

            var users = db.Users
                          .Where(u => !adminUserIds.Contains(u.Id))
                          .ToList();

            // See what users are in the current task
            var usersWithinTask = users.Select(user => new
            {
                User = user,
                IsAssignedToTask = task.Users.Contains(user)
            }).ToList();

            ViewBag.Users = users;
            ViewBag.UsersWithinTask = usersWithinTask;
            ViewBag.ProjectId = task.Id;

            var comments = task.Comments?.Select(c => new
            {
                c.Id,
                c.Text,
                c.Date,
                c.UserId,
                IsCurrentUserComment = c.UserId == currentUserId
            }).ToList();

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
        public IActionResult AllUsers(int taskId)
        {
            // Fetch the admin role ID
            var adminRoleId = db.Roles.FirstOrDefault(r => r.Name == "Admin")?.Id;
            if (adminRoleId == null)
            {
                return NotFound("Admin role doesn't exist.");
            }

            // Get the task and its associated project
            var task = db.Tasks
                         .Include(t => t.Project)
                         .Include(t => t.Users)
                         .FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            var project = db.Projects
                            .Include(p => p.Users)
                            .FirstOrDefault(p => p.Id == task.ProjectId);
            if (project == null)
            {
                return NotFound("Project not found.");
            }

            // Get the current user ID
            var currentUserId = _userManager.GetUserId(User);

            // Filter users: must be in the project, not admins
            var adminUserIds = db.UserRoles
                                 .Where(ur => ur.RoleId == adminRoleId)
                                 .Select(ur => ur.UserId)
                                 .ToList();

            var usersInProject = project.Users
                                        .Where(u => !adminUserIds.Contains(u.Id))
                                        .ToList();

            // Prepare data to determine task assignment status
            var usersWithTaskInfo = usersInProject.Select(user => new
            {
                User = user,
                IsAssignedToTask = task.Users.Contains(user)
            }).ToList();

            ViewBag.UsersWithTaskInfo = usersWithTaskInfo;
            ViewBag.TaskId = taskId;

            return View();
        }

        // POST: Add User to Task
        [HttpPost]
        [Authorize(Roles = "Member, Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult AddUserToTask(int taskId, string userId)
        {
            var task = db.Tasks.Include(t => t.Users).FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Add the user to the task
            if (!task.Users.Contains(user))
            {
                task.Users.Add(user);
                db.SaveChanges();
            }

            return RedirectToAction("AllUsers", new { taskId = taskId });
        }

        // POST: Remove User from Task
        [HttpPost]
        [Authorize(Roles = "Member, Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveUserFromTask(int taskId, string userId)
        {
            var task = db.Tasks.Include(t => t.Users).FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Remove the user from the task
            if (task.Users.Contains(user))
            {
                task.Users.Remove(user);
                db.SaveChanges();
            }

            return RedirectToAction("AllUsers", new { taskId = taskId });
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

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
using Microsoft.Build.Framework;
using System.Linq;
using System.Net.NetworkInformation;
using Luma.Migrations;

namespace Luma.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        // users and roles:
        private readonly ApplicationDbContext db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        //media:
        private readonly IWebHostEnvironment _env;
        public TasksController(
        ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment env)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
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

            // Tasks by Status and ordered by End_Date
            var tasks = project.Tasks;
            ViewBag.ToDoTasks = tasks
                .Where(t => t.Status == "To do")
                .OrderBy(t => t.End_Date)
                .ToList();
            ViewBag.InProgressTasks = tasks
                .Where(t => t.Status == "In progress")
                .OrderBy(t => t.End_Date)
                .ToList();
            ViewBag.DoneTasks = tasks
                .Where(t => t.Status == "Done")
                .OrderBy(t => t.End_Date)
                .ToList();

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
        public async Task<IActionResult> New(TaskModel task, IFormFile? Media)
        {

            if (task.End_Date <= task.Start_Date)
            {
                ModelState.AddModelError("End_Date", "End date must be later than the start date.");
            }

          

            if (Media != null && Media.Length > 0)
            {
                // Verificăm extensia
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif",".mp4", ".mov" };
                var fileExtension = Path.GetExtension(Media.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("TaskMedia", "Fișierul trebuie să fie o imagine(jpg, jpeg, png, gif) sau un video(mp4, mov)" );
                    return View(task);
                }

                // Cale stocare
                var storagePath = Path.Combine(_env.WebRootPath, "media", Media.FileName);
                var databaseFileName = "/media/" + Media.FileName;
                // Salvare fișier
                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Media.CopyToAsync(fileStream);
                }
                ModelState.Remove(nameof(task.Media));
                task.Media = databaseFileName;
            }
            else
            {
                task.Media = null;
            }

            if (Media == null)
            {
                ModelState.Remove("Media");
            }

            if (ModelState.IsValid == false)
            {
                return View(task);
            }

            db.Tasks.Add(task);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Tasks", new { projectId = task.ProjectId });


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
                c.User.UserName,
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
        public async Task<IActionResult> Edit(TaskModel task, IFormFile? newMedia, string? removeMedia)
        {
            if (!string.IsNullOrEmpty(removeMedia) && removeMedia == "true")
            {
                if (!string.IsNullOrEmpty(task.Media))
                {
                    var filePath = Path.Combine(_env.WebRootPath, task.Media.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    task.Media = null;
                }
            }
            else if (newMedia != null && newMedia.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" };
                var fileExtension = Path.GetExtension(newMedia.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Media", "File must be an image (jpg, jpeg, png, gif) or a video (mp4, mov).");
                    return View(task);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(newMedia.FileName);
                var storagePath = Path.Combine(_env.WebRootPath, "media", uniqueFileName);
                var databaseFileName = "/media/" + uniqueFileName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await newMedia.CopyToAsync(fileStream);
                }

                task.Media = databaseFileName;
            }

            if (task.End_Date <= task.Start_Date)
            {
                ModelState.AddModelError("End_Date", "End date must be later than the start date.");
            }

            if (!ModelState.IsValid)
            {
                return View(task);
            }

            db.Tasks.Update(task);
            await db.SaveChangesAsync();

            return RedirectToAction("Index", "Tasks", new { projectId = task.ProjectId });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus([FromBody] UpdateStatusRequest request)
        {
            var task = db.Tasks.Find(request.Id);
            if (task == null)
            {
                return NotFound();
            }

            task.Status = request.Status;
            db.Tasks.Update(task);
            db.SaveChanges();

            return Ok();
        }

        public class UpdateStatusRequest
        {
            public int Id { get; set; }
            public string Status { get; set; }
        }
    }
}

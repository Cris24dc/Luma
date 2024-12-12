using Luma.Data;
using Luma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskModel = Luma.Models.Task;

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
            // Project data
            var project = db.Projects.Include(p => p.Tasks).FirstOrDefault(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            // Organizer data
            var organizer = _userManager.Users.FirstOrDefault(u => u.Id == project.Organizer);

            if (organizer != null)
            {
                ViewBag.OrganizerName = $"{organizer.UserName}";
            }
            else
            {
                ViewBag.OrganizerName = "Unknown Organizer";
            }

            // Rights for CRUD
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
                Start_Date = DateTime.Now, // Data curentă
                End_Date = DateTime.Now.AddDays(1) // Data implicită pentru End_Date
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
                return RedirectToAction("Index", "Tasks", new { projectId = task.ProjectId }); // Redirect to the project page
            }

            return View(task);
        }

        // GET: Tasks/Show/5
        [Authorize]
        public IActionResult Show(int id)
        {
            var task = db.Tasks.Include(t => t.Project).FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            ViewBag.ShowButtons = task.Project.Organizer == currentUserId || User.IsInRole("Admin");

            return View(task);
        }

        // GET: Tasks/Edit/5
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

        // POST: Tasks/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TaskModel task)
        {
            if (ModelState.IsValid)
            {
                db.Tasks.Update(task);
                db.SaveChanges();
                return RedirectToAction("Index", "Tasks", new { projectId = task.ProjectId }); // Redirect to the project page
            }

            return View(task);
        }


        // POST: Tasks/Delete/5
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

            var projectId = task.ProjectId; // Get the projectId of the task to redirect to the project page
            db.Tasks.Remove(task);
            db.SaveChanges();

            return RedirectToAction("Index", "Tasks", new { projectId = projectId }); // Redirect to the project page
        }

        private void SetAccessRights(Project project)
        {
            ViewBag.ShowButtons = false;

            var currentUserId = _userManager.GetUserId(User);
            if (project.Organizer == currentUserId || User.IsInRole("Admin"))
            {
                ViewBag.ShowButtons = true;
            }
        }

        // POST: Tasks/UpdateStatus/5
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

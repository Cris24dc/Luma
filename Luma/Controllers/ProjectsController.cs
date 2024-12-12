using Luma.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Luma.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.VisualStudio.TextTemplating;

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
       RoleManager<IdentityRole> roleManager
       )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        
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
                    OrganizerUsername = project.Users
            .FirstOrDefault(u => u.Id == project.Organizer).UserName 
                }).ToList();

                ViewBag.ProjectViewModels = projectViewModels;


                return View();

            }
            else
            if(User.IsInRole("Admin"))
            {
                var projects = from project in db.Projects.Include("Users")
                               select project;
                ViewBag.Projects = projects;

                var projectViewModels = projects.Select(project => new
                {
                    Project = project,
                    OrganizerUsername = project.Users
            .FirstOrDefault(u => u.Id == project.Organizer).UserName // Find the username of the organizer
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

        [Authorize(Roles = "Member,Admin")]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Member,Admin")]
        public IActionResult New(Project p)
        {
            string userId = _userManager.GetUserId(User);

            p.Organizer = userId;

            var user = db.Users.FirstOrDefault(u => u.Id == userId);

            if (user != null)
            {
                p.Users = new List<User> { user };

                db.Projects.Add(p);
                db.SaveChanges();

              

                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "User not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }



        [Authorize(Roles = "Member,Admin")]

        public IActionResult Edit(int id)
        {
            Project project = db.Projects.Where(p => p.Id == id)
                                         .First();

            if ((project.Organizer == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                return View(project);
            }
            else
            {
                TempData["message"] = "You dont have permission to edit the project";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [Authorize(Roles ="Member,Admin")]

        public IActionResult Edit(int id, Project editproject)
        {
            Project project = db.Projects.Find(id);
            if ((project.Organizer == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                project.Project_Name = editproject.Project_Name;

                TempData["message"] = "Project was modified";
                TempData["messageType"] = "alert-succes";
                db.SaveChanges();
                return RedirectToAction("Index");   
            }
            else
            {
                TempData["message"] = "You dont have permission to edit the project";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }


        [Authorize(Roles = "Member,Admin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Project project = db.Projects.Where(p => p.Id == id)
                                         .First();

            if ((project.Organizer == _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                db.Projects.Remove(project);
                db.SaveChanges();
                TempData["message"] = "Project was deleted";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "You dont have permission to delete the project";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

        }


        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Member"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }








    }
}

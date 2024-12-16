using Luma.Data;
using Luma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Luma.Controllers
{
    public class CommentsController : Controller
    {
        // users and roles:
        private readonly ApplicationDbContext db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CommentsController(
        ApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // POST: New Action
        [HttpPost]
        [Authorize(Roles = "Member")]
        public IActionResult New(Comment comment)
        {

           
            comment.Date = DateTime.Now;
            var id = comment.TaskId;

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                comment.UserId = _userManager.GetUserId(User);
                db.SaveChanges();
                return RedirectToAction("Show", "Tasks", new { id = comment.TaskId });
            }
            else
            {
                    TempData["comment"] = "Comment cannot be null.";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Show", "Tasks", new {id = id});

            }
        }

        // POST: Delte Action
        [HttpPost]
        [Authorize(Roles = "Member,Admin")]
        public IActionResult Delete(int id)
        {
            Comment comment = db.Comments.Find(id);

            if (comment.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comment);
                db.SaveChanges();
                return RedirectToAction("Show", "Tasks", new { id = comment.TaskId });

            }
            else
            {
                TempData["message"] = "You dont have permission to delete this comment!!!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Tasks", new { id = comment.Task.ProjectId });
            }
        }

        // GET: Edit Action
        [Authorize(Roles = "Member")]
        public IActionResult Edit(int id)
        {
            Comment comment = db.Comments.Find(id);

            if (comment == null)
            {
                return NotFound();
            }

            if (comment.UserId == _userManager.GetUserId(User))
            {
                // Transmite TaskId prin ViewBag
                ViewBag.TaskId = comment.TaskId;

                return View(comment);
            }
            else
            {
                TempData["message"] = "You don't have permission to edit this comment!!!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Tasks");
            }
        }

        // POST: Edit Action
        [HttpPost]
        [Authorize(Roles = "Member")]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment comment = db.Comments.Find(id);

            if (comment.UserId == _userManager.GetUserId(User))
            {
                if (ModelState.IsValid)
                {
                    comment.Text = requestComment.Text;

                    db.SaveChanges();

                    return RedirectToAction("Show", "Tasks", new { id = comment.TaskId });
                }
                else
                {
                    return View(requestComment);
                }
            }
            else
            {
                TempData["message"] = "You dont have permission to edit this comment!!!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Tasks", new { id = comment.Task.ProjectId });
            }
        }
    }
}

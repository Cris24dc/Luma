using Luma.Data;
using Luma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Luma.Controllers
{


    public class CommentsController : Controller
    {
        //Useri si Roluri

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



        [HttpPost]
        //[Authorize(Roles = "Membru,Organizator,Admin")]
        public IActionResult Delete(int id)
        {
            Comment comment = db.Comments.Find(id);

            if (comment.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comment);
                db.SaveChanges();
                return Redirect("/Projects/Show/" + comment.TaskId);

            }
            else
            {
                TempData["message"] = "You dont have permission to delete this comment!!!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Projects");
            }

        }

        //[Authorize(Roles = "Membru,Organizator")]
        public IActionResult Edit(int id)
        {
            Comment comment = db.Comments.Find(id);

            if (comment.UserId == _userManager.GetUserId(User))
            {
                return View(comment);
            }
            else
            {
                TempData["message"] = "You dont have permission to edit this comment!!!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Projects");
            }
        }

        [HttpPost]
        //[Authorize(Roles = "Membru,Organizator")]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment comment = db.Comments.Find(id);

            if (comment.UserId == _userManager.GetUserId(User))
            {
                if (ModelState.IsValid)
                {
                    comment.Text = requestComment.Text;

                    db.SaveChanges();

                    return Redirect("/Projects/Show" + comment.TaskId);
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
                return RedirectToAction("Index", "Projects");
            }
        }
    }

}

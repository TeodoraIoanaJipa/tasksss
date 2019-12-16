using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tasks.Models;

namespace Tasks.Controllers
{
    public class CommentController : Controller
    {

        private ApplicationDbContext database = new ApplicationDbContext();
        // GET: Comment
        public ActionResult Index()
        {
            var comments = database.Comments;
            ViewBag.Comments = comments;
            return View();
        }

        [HttpPost]
        public ActionResult New(Comment comment)
        {
            try
            {
                Console.WriteLine(comment.Content);
                comment.LeftById = User.Identity.GetUserId();
                database.Comments.Add(comment);
                TempData["message"] = "Comment was succesfully added.";
                database.SaveChanges();
                return Redirect("/Task/Show/" + comment.TaskId);
                
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = e.Message;
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "User,Organizer,Member,Administrator")]
        public ActionResult Edit(int id)
        {
            Comment comment = database.Comments.Find(id);
            var taskId = comment.TaskId;

            if (comment.LeftById == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {              
                return View(comment);
            }
            else
            {
                TempData["message"] = "You can't edit a project that was not created by you!";
                return Redirect("/Task/Show/" + taskId);
            }

        }

        [HttpPut]
        public ActionResult Edit(int id, Comment requestedComment)
        {
            try
            {
                if (ModelState.IsValid) {

                    Comment comment = database.Comments.Find(id);
                    
                    var taskId = comment.TaskId;
                    if (TryUpdateModel(comment))
                    {
                        comment.Content = requestedComment.Content;
                        comment.CreatedDate = DateTime.Now;
                        comment.LeftById = User.Identity.GetUserId();

                        database.SaveChanges();
                    }
                    TempData["message"] = "Comment was succesfully edited.";
                    return Redirect("/Task/Show/" + taskId);
                }
                else
                {
                    return View(requestedComment);
                }
            }
            catch (Exception e) {
                ViewBag.ErrorMessage = e.Message;
                return View("Error");
            }
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Comment comment = database.Comments.Find(id);
            var taskId = comment.TaskId;
            database.Comments.Remove(comment);
            database.SaveChanges();
            TempData["message"] = "Comment " + comment.Content + " was succesfully deleted."; 
            return Redirect("/Task/Show/" + taskId);
        }
    }
}
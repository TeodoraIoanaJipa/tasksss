using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tasks.Models;

namespace Tasks.Controllers
{
    [Authorize(Roles = "User,Administrator")]
    public class TaskController : Controller
    {
        ApplicationDbContext database = new ApplicationDbContext();

        [NonAction]
        private IEnumerable<SelectListItem> GetAllUserFromTeam(int teamid)
        {
            var selectList = new List<SelectListItem>();

            var allTeams = from teams in database.Teams
                           where teams.TeamId == teamid
                           select teams;
            var allUsers = from usr in database.Users
                           where usr.Teams.Contains(allTeams.FirstOrDefault())
                           select usr;


            foreach (var user in allUsers.ToArray())
            {
                SelectListItem item = new SelectListItem
                {
                    Value = user.Id.ToString(),
                    Text = user.UserName.ToString()
                };
                selectList.Add(item);
            }

            return selectList;
        }

        public ActionResult Display(int id)
        {
            ViewBag.displayEdit = true;
            
            return Redirect("/Task/Show/" + id);

        }
        [HttpGet]
        [Authorize(Roles = "User,Organizer,Member,Administrator")]
        public ActionResult Index()
        {
            ViewBag.Tasks = database.Tasks.Include("AssignedTo");
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Organizer,Member,Administrator")]
        public ActionResult AddTask(int id)
        {
            Task task = new Task();
            Project project = database.Projects.Find(id);
            ViewBag.ProjectId = id;
            task.ProjectId = id;

            if (GetAllUserFromTeam(project.TeamId) != null)
            {
                task.Users = GetAllUserFromTeam(project.TeamId);
            }
            else
            {
                task.Users = new List<SelectListItem>();
                ViewBag.ErrorMessage = "The organizer has no team";
                return View("Error");
            }

            return View("New", task);
        }

        [HttpPost]
        [Authorize(Roles = "Organizer,Member,Administrator")]
        public ActionResult New(Task task)
        {
            Project project = database.Projects.Find(task.ProjectId);
            task.Users = GetAllUserFromTeam(project.TeamId);
            //Debug.Print(id.ToString()+"add task");
            try
            {
                if (User.IsInRole("Organizer") && User.Identity.GetUserId() == project.OrganizerId || User.IsInRole("Administrator"))
                {
                    if (ModelState.IsValid)
                    {
                        TempData["message"] = "Task " + task.Title + " was succesfully added.";
                        database.Tasks.Add(task);
                        database.SaveChanges();
                        return Redirect("/Project/Show/" + task.ProjectId);
                    }
                    else
                    {
                        ViewBag.ProjectId = task.ProjectId;
                        return View(task);
                    }
                }
                else
                {
                    TempData["message"] = "You can't edit a project that was not created by you!";
                    return View(task);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + e.StackTrace);
                ViewBag.ProjectId = task.ProjectId;
                return View();
            }
        }


        [HttpGet]
        public ActionResult Show(int id)
        {
            var task = database.Tasks.Find(id);
            ViewBag.comments = task.Comments;

            ViewBag.displayEdit = false;

            Project proj = database.Projects.Find(task.ProjectId);
            ViewBag.showButtons = false;

            ViewBag.CurrentUser = User.Identity.GetUserId();
            ApplicationUser user = database.Users.Find(ViewBag.CurrentUser);
            

            var allTeams = from teams in database.Teams
                           where teams.TeamId == proj.TeamId
                           select teams;
            var allUsers = from usr in database.Users
                           where usr.Teams.Contains(allTeams.FirstOrDefault())
                           && usr.Id == user.Id
                           select usr;
            

            if ((User.IsInRole("Organizer") && proj.OrganizerId == User.Identity.GetUserId()) || User.IsInRole("Administrator") ||
                (User.IsInRole("Member") && allUsers.Count() != 0))
            {
                ViewBag.showButtons = true;
            }

            
            return View(task);
        }
        [HttpGet]
        [Authorize(Roles = "User,Organizer,Member,Administrator")]
        public ActionResult Edit(int id)
        {
            Task task = database.Tasks.Find(id);
                   
            var projectId = task.ProjectId;
            Project project = database.Projects.Find(projectId);

            task.Users = GetAllUserFromTeam(project.TeamId);

            if (project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                return View(task);
            }
            else
            {
                TempData["message"] = "You can't edit a task that was not created by you!";
                return RedirectToAction("Index");
            }

        }
        [HttpPut]
        public ActionResult Edit(int id, Task requestTsk)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Task task = database.Tasks.Find(id);
                    int projectId = task.ProjectId;
                    if (TryUpdateModel(task))
                    {
                        task.Title = requestTsk.Title;
                        task.TaskDescription = requestTsk.TaskDescription;
                        task.Status = requestTsk.Status;
                        task.StartDate = requestTsk.StartDate;
                        task.EndDate = requestTsk.EndDate;
                        task.Users = GetAllUserFromTeam(projectId);
                        task.AssignedToId = requestTsk.AssignedToId;
                        database.SaveChanges();
                    }
                    TempData["message"] = "Task was succesfully modified!";
                    return Redirect("/Project/Show/" + projectId);
                }
                else
                {
                    return View(requestTsk);
                }
            }
            catch (Exception e) {
                ViewBag.ErrorMessage = e.Message + e.StackTrace;
                return View("Error");
            }
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Task task = database.Tasks.Find(id);
            var projectId = task.ProjectId;
            TempData["message"] = "Task " + task.Title + " was succesfully deleted.";
            database.Tasks.Remove(task);
            database.SaveChanges();
            return Redirect("/Project/Show/" + projectId);
        }
    }
}
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tasks.ActionFilters;
using Tasks.Models;

namespace Tasks.Controllers
{
    [Authorize(Roles = "User,Administrator")]
    public class ProjectController : Controller
    {
        private ApplicationDbContext database = new ApplicationDbContext();

        [NonAction]
        public IQueryable<Project> GetProjects()
        {
            if (User.IsInRole("Administrator"))
            {
                var projects = from project in database.Projects.Include("Organizer").Include("Team")
                               select project;
                return projects;
            }
            else
            {
                var currentUser = User.Identity.GetUserId();
                var user = from u in database.Users
                           where u.Id == currentUser
                           select u;

                var projects = from project in database.Projects.Include("Organizer").Include("Team")
                               where project.OrganizerId == currentUser
                               || project.Team.Members.Contains(user.FirstOrDefault())
                               select project;
                return projects;
            }
                 
        }

        [LogFilter]
        [HttpGet]
        [Authorize(Roles = "User,Organizer,Member,Administrator")]
        public ActionResult Index()
        {
            ViewBag.CurrentUser = User.Identity.GetUserName();
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            var projects = GetProjects();
            ViewBag.Projects = projects;
            return View();
        }

        [LogFilter]
        [HttpGet]
        [Authorize(Roles = "User,Administrator")]
        public ActionResult Show(int id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            try
            {
                Project project = database.Projects.Find(id);

                var tasks = from todo in project.Tasks
                            select todo;
                var users = from usr in project.Team.Members
                            select usr;
                if (tasks.Count() != 0)
                {
                    TempData["tasks"] = tasks;
                }
                ViewBag.Taskuri = tasks;
                ViewBag.TasksCount = tasks.Count();
                ViewBag.CurrentUser = User.Identity.GetUserName();
                ViewBag.Useri = users;
                ViewBag.showButtons = false;
                if (project.OrganizerId == User.Identity.GetUserId()||
                    User.IsInRole("Administrator"))
                {
                    ViewBag.showButtons = true;
                }

                return View(project);
            }
            catch (Exception exp)
            {
                ViewBag.ErrorMessage = exp.Message;
                return View("Error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Organizer,Member,Administrator")]
        public int NewTeam(Project project)
        {
            Team team = new Team();
            team.Name = project.Title;
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(database));
            //adaugi userii
            ICollection<ApplicationUser> appUsers = new List<ApplicationUser>();
            foreach (var id in project.SelectedUserIds)
            {
                ApplicationUser applicationUser = database.Users.Find(id);
                UserManager.AddToRole(id, "Member");
                appUsers.Add(applicationUser);
            }
            team.Members = appUsers;
            database.Teams.Add(team);
            database.SaveChanges();
            return team.TeamId;
        }

        [HttpGet]
        [Authorize(Roles = "User,Organizer,Member,Administrator")]
        public ActionResult New()
        {
            Project project = new Project();
            TeamController teamController = new TeamController();
            project.Users = teamController.GetUsers();
            project.OrganizerId = User.Identity.GetUserId();
            return View(project);
        }


        [HttpPost]
        [Authorize(Roles = "User,Organizer,Member,Administrator")]
        public ActionResult New(Project project)
        {
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(database));
            try
            {
                if (ModelState.IsValid)
                {
                    var teamId = NewTeam(project);
                    project.TeamId = teamId;
                    database.Projects.Add(project);
                    database.SaveChanges();//commit
                    UserManager.AddToRole(project.OrganizerId, "Organizer");
                    database.SaveChanges();
                    TempData["message"] = "Project " + project.Title + " was succesfully added.";
                    return Redirect("/Project/Index");
                }
                else
                {
                    return View(project);
                }
            }
            catch (Exception exception)
            {
                ViewBag.ErrorMessage = exception.Message;
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "User,Organizer,Member,Administrator")]
        public ActionResult Edit(int id)
        {
            Project p = database.Projects.Find(id);
            if (p.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                ViewBag.Project = p;
                return View(p);
            }
            else
            {
                TempData["message"] = "You can't edit a project that was not created by you!";
                return RedirectToAction("Index");
            }

        }

        // PUT: vrem sa trimitem modificarile la server si sa le salvam
        [HttpPut]
        [Authorize(Roles = "User,Organizer,Member,Administrator")]
        public ActionResult Edit(int id,Project project)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Project p = database.Projects.Find(id);
                    if (p.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {

                        if (TryUpdateModel(p))
                        {
                            p.Title = project.Title;
                            p.Description = project.Description;
                            p.Organizer = project.Organizer;
                            p.CreatedDate = DateTime.Now;
                            database.SaveChanges();
                        }
                        TempData["message"] = "Project was succesfully edited.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "You can't edit a project that was not created by you!";
                        return RedirectToAction("Index");
                    }

                }
                else
                {
                    return View(project);
                }
            }
            catch (Exception exception)
            {
                ViewBag.ErrorMessage = exception.Message;
                return View("Error");
            }

        }


        [HttpDelete]
        [Authorize(Roles = "User,Organizer,Member,Administrator")]
        public ActionResult Delete(int id)
        {
            Project project = database.Projects.Find(id);
            if (project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                TempData["message"] = "Project " + project.Title + " was succesfully deleted.";
                database.Projects.Remove(project);
                database.SaveChanges();
                return Redirect("/Project/Index");
            }
            else
            {
                TempData["message"] = "You can't edit a project that was not created by you!";
                return RedirectToAction("Index");
            }

        }
    }
}
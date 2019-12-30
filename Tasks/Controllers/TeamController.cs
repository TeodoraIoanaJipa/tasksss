using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Tasks.Models;

namespace Tasks.Controllers
{
    public class TeamController : Controller
    {
        private ApplicationDbContext database = new ApplicationDbContext();

        [NonAction]
        public IEnumerable<SelectListItem> GetUsers()
        {
            // generam o lista goala             
            var selectList = new List<SelectListItem>();
            var users = database.Users;

            foreach (var user in users)
            {
                // Adaugam in lista elementele necesare pentru dropdown                 
                selectList.Add(new SelectListItem
                {
                    Value = user.Id.ToString(),
                    Text = user.UserName.ToString(),
                    Selected = false
                });
            }

            return selectList;
        }

        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            string id = User.Identity.GetUserId();
            if (User.IsInRole("Administrator"))
            {
                var teams = database.Teams;
                ViewBag.Teams = teams;
                
            }
            else
            {
                var user = from u in database.Users
                           where u.Id == id
                           select u;
                var teams = from team in database.Teams
                            where team.Members.Contains(user.FirstOrDefault())
                            select team;
                ViewBag.Teams = teams;          
            }
            return View();
        }


        public ActionResult Show(int id)
        {
            var team = database.Teams.Find(id);
            ViewBag.Tasks = from tsk in database.Tasks.Include("Project")
                            where tsk.Project.TeamId == id
                            select tsk;
            return View(team);
        }

        public ActionResult New()
        {
            Team team = new Team();
            team.Users = GetUsers();


            //ViewBag.Users = new MultiSelectList(users, "UserId", "UserName");
            //ViewBag.Users = GetUsers();
            return View(team);
        }

        [HttpPost]
        public ActionResult New(Team team)
        {
            try
            {
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(database));
                //adaugi userii
                ICollection<ApplicationUser> appUsers = new List<ApplicationUser>();
                foreach (var id in team.SelectedUserIds)
                {
                    ApplicationUser applicationUser = database.Users.Find(id);
                    UserManager.AddToRole(id, "Member");
                    appUsers.Add(applicationUser);
                }
                team.Members = appUsers;
                database.Teams.Add(team);
                database.SaveChanges();
                //editezi userii
                foreach (var id in team.SelectedUserIds)
                {
                    ApplicationUser applicationUser = database.Users.Find(id);
                    if (TryUpdateModel(applicationUser))
                    {
                        //applicationUser.TeamId = team.TeamId;
                        database.SaveChanges();
                    }
                }
                TempData["message"] = "Team " + team.Name + " was succesfully added.";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            Team team = database.Teams.Find(id);
            team.Users = GetUsers();
            List<string> userIds = new List<string>();        
            foreach (var usr in team.Members)
            {
                userIds.Add(usr.Id);
            }
            team.SelectedUserIds = userIds.ToArray();
            return View(team);
        }


        [HttpPut]
        public ActionResult Edit(int id, Team requestTeam)
        {
            try
            {
                Team team = database.Teams.Find(id);
                ICollection<ApplicationUser> appUsers = new List<ApplicationUser>();
                
                if (TryUpdateModel(team))
                { 
                    team.Name = requestTeam.Name;
                    foreach (var i in requestTeam.SelectedUserIds)
                    {
                        ApplicationUser applicationUser = database.Users.Find(i);
                        //appUsers.Add(applicationUser);
                        team.Members.Add(applicationUser);
                    }            
                    database.SaveChanges();
                }
                TempData["message"] = "Team " + team.Name + " was succesfully edited.";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View();
            }
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Team team = database.Teams.Find(id);
            TempData["message"] = "Team " + team.Name + " was succesfully deleted.";
            database.Teams.Remove(team);
            database.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
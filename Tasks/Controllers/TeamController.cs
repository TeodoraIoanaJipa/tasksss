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
            var teams = database.Teams;
            ViewBag.Teams = teams;
            return View();
        }


        public ActionResult Show(int id)
        {
            var team = database.Teams.Find(id);
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
            return View(team);
        }


        [HttpPut]
        public ActionResult Edit(int id, Team requestTeam)
        {
            try
            {
                Team team = database.Teams.Find(id);
                if (TryUpdateModel(team))
                {
                    team.Name = requestTeam.Name;
                    team.Members = requestTeam.Members;
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
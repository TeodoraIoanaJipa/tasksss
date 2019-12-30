using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tasks.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;

namespace Tasks.Controllers
{
    public class UserController : Controller
    {
        private ApplicationDbContext database = new ApplicationDbContext();
        // GET: User
        public ActionResult Index()
        {
            var users = from u in database.Users
                        select u;
            ViewBag.Users = users;
            return View();
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();
            var roles = from role in database.Roles select role;
            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }

        public ActionResult Edit(string id)
        {
            ApplicationUser user = database.Users.Find(id);
            user.AllRoles = GetAllRoles();
            ViewBag.userRole = user.Roles.FirstOrDefault().RoleId;
            return View(user);
        }

        [HttpPut]
        public ActionResult Edit(string id, ApplicationUser newData)
        {
            ApplicationUser user = database.Users.Find(id);
            user.AllRoles = GetAllRoles();
            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId;
            try
            {
                ApplicationDbContext context = new ApplicationDbContext(); var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context)); var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                if (TryUpdateModel(user))
                {
                    user.UserName = newData.UserName; user.Email = newData.Email; user.PhoneNumber = newData.PhoneNumber;

                    var roles = from role in database.Roles
                                select role;
                    foreach (var role in roles)
                    {
                        UserManager.RemoveFromRole(id, role.Name);
                    }

                    var selectedRole = database.Roles.Find(HttpContext.Request.Params.Get("newRole"));
                    UserManager.AddToRole(id, selectedRole.Name);
                    database.SaveChanges();
                    TempData["message"] = "User was succesfully edited.";
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
                return View(user);
            }
        }

        public async System.Threading.Tasks.Task<ActionResult> Show(string id)
        {
            ApplicationUser user = database.Users.Find(id);

            using (
            var userManager =
                new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
            {
                ViewBag.rolesForUser = await userManager.GetRolesAsync(id);
                return View(user);
            }
        }
    }
}
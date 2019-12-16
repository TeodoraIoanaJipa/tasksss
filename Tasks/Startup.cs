using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using System.Linq;
using Tasks.Models;

[assembly: OwinStartupAttribute(typeof(Tasks.Startup))]
namespace Tasks
{
    public partial class Startup
    {
        private ApplicationDbContext database = new ApplicationDbContext();
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            createApplicationRoles();
        }

        private void createApplicationRoles()
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!roleManager.RoleExists("Administrator"))
            {
                var role = new IdentityRole();
                role.Name = "Administrator";
                roleManager.Create(role);

                var team = new Team();
                team.Name = "management";
                database.Teams.Add(team);
                database.SaveChanges();

                // se adauga utilizatorul administrator
                var user = new ApplicationUser();
                user.UserName = "admin@yahoo.com";
                user.Email = "admin@yahoo.com";
                //user.TeamId = team.TeamId;

                var adminCreated = UserManager.Create(user, "Admin1!");
                if (adminCreated.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Administrator");
                }
            }

            if (!roleManager.RoleExists("Organizer"))
            {
                var role = new IdentityRole();
                role.Name = "Organizer";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Member"))
            {
                var role = new IdentityRole();
                role.Name = "Member";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("User"))
            {
                var role = new IdentityRole();
                role.Name = "User";
                roleManager.Create(role);
            }
        }
    }
}

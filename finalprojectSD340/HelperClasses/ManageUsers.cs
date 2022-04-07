using finalprojectSD340.Data;
using finalprojectSD340.Models;
using Microsoft.AspNetCore.Identity;

namespace finalprojectSD340.HelperClasses
{
    public class ManageUsers
    {
        private ApplicationDbContext Db { get; set; }
        private UserManager<ApplicationUser> UserManager { get; set; }
        private RoleManager<IdentityRole> RoleManager { get; set; }

        public ManageUsers(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            Db = db;
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public async Task<IList<string>> GetAllRoles(string id)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(id);
            IList<string> userRoles = await UserManager.GetRolesAsync(user);
            return userRoles;
        }

        public async Task<string> AssignRole(string userId, string roleId)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(userId);
            IdentityRole role = await RoleManager.FindByIdAsync(roleId);
            if (await CheckForRole(userId, roleId) == false)
            {
                await UserManager.AddToRoleAsync(user, role.Name);
                Db.SaveChanges();
                return $"The role {role.Name} was added to {user.UserName}";
            }
            return $"{user.UserName} is already in the role {role.Name}";
        }

        public async Task<bool> CheckForRole(string userId, string roleId)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(userId);
            IdentityRole role = await RoleManager.FindByIdAsync(roleId);
            return await UserManager.IsInRoleAsync(user, role.Name);
        }
    }
}
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTRolesService : IBTRolesService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BTUser> _userManager;

        #region Constructor
        public BTRolesService(ApplicationDbContext context,
    RoleManager<IdentityRole> roleManager,
    UserManager<BTUser> userManager
    )
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        #endregion

        #region Get Roles
        public async Task<List<IdentityRole>> GetRolesAsync()
        {
            try
            {
                List<IdentityRole> result = new();

                result = await _context.Roles.ToListAsync();

                return result;
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        #endregion

        #region Add User To Role
        public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
        {
            bool result = (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
            return result;
        }

        #endregion

        public async Task<string> GetRoleByNameIdAsync(string roleId)
        {
            IdentityRole role = _context.Roles.Find(roleId); //if I have an ID of an item I can use the .Find
            string result = await _roleManager.GetRoleNameAsync(role);
            return result;
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(BTUser user)
        {
            IEnumerable<string> result = await _userManager.GetRolesAsync(user);
            return result;
        }

        public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            List<BTUser> users = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
            //second parameter - only give the user the info for the company they're part of
            List<BTUser> result = users.Where(u => u.CompanyId == companyId).ToList();
            return result;
        }

        public async Task<List<BTUser>> GetUsersNotInRoleAsync(string roleName, int companyId)
        {
            //if I'm looking for project manager role - I need to find everybody that is a project manager
            //save their ids into a list and then find all users that are not contained in the projectM list

            //userIds in IdentityUser are guids - we need to read and get this first
            List<string> userIds = (await _userManager.GetUsersInRoleAsync(roleName)).Select(u => u.Id).ToList();
            //if I take a group of data - Where is going to check and return multiples; set of data
            //(blanket statement - for every one of those records in the db table that may match whatever I provide
            //then I want them in my result)
            List<BTUser> roleUsers = _context.Users.Where(u => !userIds.Contains(u.Id)).ToList();
            //lambda above - for every user record in the db, check that user record and see if it's not contained 
            //in the list of userIds that I found on the line above (can use .Contains on a list only)

            List<BTUser> result = roleUsers.Where(u => u.CompanyId == companyId).ToList();
            return result;
        }

        public async Task<bool> IsUserInRoleAsync(BTUser user, string roleName)
        {
            bool result = await _userManager.IsInRoleAsync(user, roleName);
            return result;
        }

        public async Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
        {
            bool result = (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;
            return result;
        }

        public async Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roles)
        {
            bool result = (await _userManager.RemoveFromRolesAsync(user, roles)).Succeeded;
            return result;
        }
    }
}

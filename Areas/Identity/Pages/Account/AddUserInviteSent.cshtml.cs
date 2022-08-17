using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class AddUserInviteSent : PageModel
    {
        private readonly UserManager<BTUser> _userManager;

        public AddUserInviteSent(UserManager<BTUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return Page();
        }

    }
}

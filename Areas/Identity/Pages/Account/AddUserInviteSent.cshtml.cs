using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BugTracker.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class AddUserInviteSent : PageModel
    {
        public void OnGet()
        {
        }
    }
}

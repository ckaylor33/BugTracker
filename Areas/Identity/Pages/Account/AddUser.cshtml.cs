using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using BugTracker.Services.Interfaces;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using BugTracker.Extensions;
using BugTracker.Models.Enums;

namespace BugTracker.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class AddUserModel : PageModel
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTRolesService _rolesService;
        private readonly SignInManager<BTUser> _signInManager;

        public AddUserModel(UserManager<BTUser> userManager,
                            IEmailSender emailSender,
                            ILogger<RegisterModel> logger,
                            IBTCompanyInfoService companyInfoService,
                            IBTRolesService rolesService,
                            SignInManager<BTUser> signInManager)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _companyInfoService = companyInfoService;
            _rolesService = rolesService;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }


        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Identity/Account/AddUser");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                int companyId = User.Identity.GetCompanyId().Value;
                string companyName = (await _companyInfoService.GetCompanyInfoByIdAsync(companyId)).Name;

                var user = new BTUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Company = await _companyInfoService.AssignNewUserToCompany(companyName),
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user);

                if ((await _rolesService.GetUsersInRoleAsync(nameof(Roles.Admin), user.Company.Id)).Count == 0)
                {
                    await _rolesService.AddUserToRoleAsync(user, nameof(Roles.Admin));

                }
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account without password.");

                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/InviteConfirm",
                        pageHandler: null,
                        values: new { area = "Identity", code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Company Invite",
                        $"You have been invited to join {companyName}. Enter and confirm your new password to join by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    return RedirectToPage("./AddUserInviteSent");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}

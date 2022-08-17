using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using BugTracker.Services.Interfaces;
using BugTracker.Data;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace BugTracker.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class InviteConfirmModel : PageModel
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public InviteConfirmModel(UserManager<BTUser> userManager, IBTFileService fileService, ApplicationDbContext context, IConfiguration config)
        {
            _userManager = userManager;
            _fileService = fileService;
            _context = context;
            _config = config;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string Code { get; set; }

            [NotMapped]
            [DataType(DataType.Upload)]
            public IFormFile AvatarImage { get; set; }

            [Display(Name = "Avatar")]
            public string AvatarFileName { get; set; }
            public byte[] AvatarFileData { get; set; }

            [Display(Name = "File Extension")]
            public string AvatarContentType { get; set; }
        }

        public IActionResult OnGet(string code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
                };
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToPage("./InviteLoginConfirmation");
            }


            if (Input.AvatarImage != null)
            {
                user.AvatarFileData = await _fileService.ConvertFileToByteArrayAsync(Input.AvatarImage);

                user.AvatarFileName = Input.AvatarImage.FileName;
                user.AvatarContentType = Input.AvatarImage.ContentType;

                var userAvatar = await _userManager.UpdateAsync(user);

                foreach (var error in userAvatar.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                user.AvatarFileData = await _fileService.EncodeImageAsync(_config["DefaultUserImage"]);
                user.AvatarFileName = Path.GetFileNameWithoutExtension(_config["DefaultUserImage"]);
                user.AvatarContentType = Path.GetExtension(_config["DefaultUserimage"]);

                var userAvatar = await _userManager.UpdateAsync(user);

                foreach (var error in userAvatar.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await _context.SaveChangesAsync();
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);

            if (result.Succeeded)
            {
                return RedirectToPage("./InviteLoginConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}

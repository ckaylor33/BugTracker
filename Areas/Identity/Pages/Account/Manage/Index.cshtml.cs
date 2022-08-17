﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BugTracker.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly SignInManager<BTUser> _signInManager;
        private readonly IBTFileService FileService;

        public IndexModel(
            UserManager<BTUser> userManager,
            SignInManager<BTUser> signInManager,
            IBTFileService fileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            FileService = fileService;
        }

        public string Username { get; set; }

        public string CurrentImage { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            public IFormFile Image { get; set; }
        }

        private async Task LoadAsync(BTUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            if (user.AvatarFileData is not null)
            {
                CurrentImage = FileService.ConvertByteArrayToFile(user.AvatarFileData, user.AvatarContentType);
            }

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user.AvatarFileData is not null)
            {
                CurrentImage = FileService.ConvertByteArrayToFile(user.AvatarFileData, user.AvatarContentType);
            }

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            //If and only if the user selected a new image will I update their profile
            if (Input.Image != null)
            {
                user.AvatarFileData = await FileService.ConvertFileToByteArrayAsync(Input.Image);
                user.AvatarFileName = Input.Image.FileName;
                user.AvatarContentType = Input.Image.ContentType;
            }

            await _userManager.UpdateAsync(user);

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}

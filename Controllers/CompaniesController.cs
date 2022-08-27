using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using BugTracker.Extensions;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models.Enums;
using BugTracker.Models.ViewModels;

namespace BugTracker.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTFileService _fileService;

        public CompaniesController(ApplicationDbContext context, UserManager<BTUser> userManager, IBTCompanyInfoService companyInfoService, IBTRolesService rolesService, IBTFileService fileService) //constructor - how we instantiate a class
        {
            _context = context;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
            _rolesService = rolesService;
            _fileService = fileService;
        }

        // GET: Companies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Companies.ToListAsync());
        }

        // GET: Companies/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id) //actions are what provide the user a view
        {
            int companyId = User.Identity.GetCompanyId().Value;

            if (id == null)
            {
                return NotFound();
            }

            var company = await _companyInfoService.GetCompanyInfoByIdAsync(companyId);

            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            AddCompanyWithUserVM model = new();

            return View(model);
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(Roles.Admin))]
        public async Task<IActionResult> Create(AddCompanyWithUserVM model)
        {
            BTUser bTUser = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid && bTUser.Email == "kaylor33@hotmail.co.uk")
            {
                await _companyInfoService.AddCompanyAsync(model.Company);

                BTUser newUser = new BTUser
                {
                    Email = model.NewUser.Email,
                    UserName = model.NewUser.Email,
                    FirstName = model.NewUser.FirstName,
                    LastName = model.NewUser.LastName,
                    EmailConfirmed = true,
                    CompanyId = model.Company.Id,
                };

                try
                {
                    if (model.NewUser.AvatarFormFile != null)
                    {
                        newUser.AvatarFileData = await _fileService.ConvertFileToByteArrayAsync(model.NewUser.AvatarFormFile);
                        newUser.AvatarFileName = model.NewUser.AvatarFormFile.FileName;
                        newUser.AvatarContentType = model.NewUser.AvatarFormFile.ContentType;
                    }

                    if (model.Company.Members.Count() == 0)
                    {
                        await _userManager.CreateAsync(newUser, "Abc&123!");
                        await _rolesService.AddUserToRoleAsync(newUser, nameof(Roles.Admin));
                        model.Company.Members.Add(newUser);

                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("*************  ERROR  *************");
                    Console.WriteLine("Error adding new user to new company.");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("***********************************");
                    throw;
                }
            }
            return View(model.Company);
        }

        // GET: Companies/Edit/5
        [Authorize(Roles = nameof(Roles.Admin))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Be explicit - it's a Company so call it that instead of var company
            Company company = await _context.Companies.FindAsync(id);

            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = company.Id });
            }
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Company company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Company company = await _context.Companies.FindAsync(id);
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}

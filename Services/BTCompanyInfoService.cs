using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTCompanyInfoService : IBTCompanyInfoService
    {
        private readonly ApplicationDbContext _context;

        public BTCompanyInfoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddCompanyAsync(Company company)
        {
            try
            {
                await _context.AddAsync(company);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Company> AssignNewUserToCompany(string companyName)
        {
            companyName = companyName.ToLower();

            List<Company> companies = await _context.Companies.ToListAsync();

            //CHECK IF COMPANY ALREADY EXISTS, ASSIGN USER TO COMPANY IF IT DOES
            foreach (Company company in companies)
            {
                if (companyName == company.Name.ToLower())
                {
                    return company;
                }
            }

            Company newCompany = new();

            newCompany.Name = companyName;

            await AddCompanyAsync(newCompany);

            //await  _rolesService.AddUserToRoleAsync(User, nameof(Roles.Admin));

            return newCompany;

        }

        public async Task<List<BTUser>> GetAllMembersAsync(int companyId)
        {
            // new() detects the type that needs to be returned - will create a new List of BTUser for us
            List<BTUser> result = new();

            //Where() is our blenket keyword - covers entire db table
            return result = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();

        }

        public async Task<List<Project>> GetAllProjectsAsync(int companyId)
        {
            List<Project> result = new();

            //if include clause was not used below; member, tickets & ProjectPriority would not be accessible.
            //Would be null unless included like this
            return result = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                   .Include(p => p.Members)
                                                   .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.Comments)
                                                   .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.Attachments)
                                                   .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.History)
                                                   .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.DeveloperUser)
                                                   .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.OwnerUser)
                                                   .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.Notifications)
                                                   .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.TicketStatus)
                                                   .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.TicketPriority)
                                                   .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.TicketType)
                                                   .Include(p => p.ProjectPriority)
                                                   .ToListAsync();
        }

        public async Task<List<Ticket>> GetAllTicketsAsync(int companyId)
        {
            List<Ticket> result = new();
            List<Project> projects = new();

            projects = await GetAllProjectsAsync(companyId);

            //SelectMany - list of projects, in that list we have a list of some other item that we're looking
            //for - in this case the Tickets
            return result = projects.SelectMany(p => p.Tickets).ToList();
        }

        public async Task<Company> GetCompanyInfoByIdAsync(int? companyId)
        {
            Company result = new();

            if (companyId != null)
            {
                result = await _context.Companies.Include(c => c.Projects)
                                                 .Include(c => c.Members)
                                                 .Include(c => c.Invites)
                                                 .FirstOrDefaultAsync(c => c.Id == companyId);
            }
            return result;
        }
    }
}

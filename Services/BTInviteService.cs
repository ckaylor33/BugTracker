using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTInviteService : IBTInviteService
    {
        private readonly ApplicationDbContext _context;

        public BTInviteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId)
        {
            //use guid to create token that will identify the sent invite according to the company
            Invite invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);

            if (invite == null)
            {
                return false;
            }

            try
            {
                //because we are accepting the invite, it is no longer valid for anyone else to use
                invite.IsValid = false;
                invite.InviteeId = userId;
                /*invite.CompanyId = companyId; //? on whether to add*/
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddNewInviteAsync(Invite invite)
        {
            try
            {
                await _context.Invites.AddAsync(invite);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AnyInviteAsync(Guid token, string email, int companyId)
        {
            //get list of invites then check against them
            try
            {
                bool result = await _context.Invites.Where(i => i.CompanyId == companyId)
                                                    .AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email);

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Invite> GetInviteAsync(int inviteId, int companyId)
        {
            try
            {
                Invite invite = await _context.Invites.Where(i => i.CompanyId == companyId)
                                                      .Include(i => i.Company)
                                                      .Include(i => i.Project)
                                                      .Include(i => i.Invitor)
                                                      .FirstOrDefaultAsync(i => i.Id == inviteId);

                return invite;
            }
            catch (Exception)
            {

                throw;
            }
        }

        //Overload through parameters
        public async Task<Invite> GetInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                Invite invite = await _context.Invites.Where(i => i.CompanyId == companyId)
                                                      .Include(i => i.Company)
                                                      .Include(i => i.Project)
                                                      .Include(i => i.Invitor)
                                                      .FirstOrDefaultAsync(i => i.CompanyToken == token && i.InviteeEmail == email);

                return invite;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            if (token == null)
            {
                return false;
            }

            bool result = false;

            Invite invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);

            if (invite != null)
            {
                //Determine invite date - convert from DateTimeOffset to DateTime
                DateTime inviteDate = invite.InviteDate.DateTime;

                //Custom validation of invite based on the date it was issued
                //In this case we are allowing an invite to be valid for 7 days (could be anything depending on spec)
                bool validDate = (DateTime.Now - inviteDate).TotalDays <= 7;

                if (validDate)
                {
                    result = invite.IsValid;
                }
            }
            return result;
        }
    }
}

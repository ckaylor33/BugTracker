using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTLookupService : IBTLookupService
    {
        private readonly ApplicationDbContext _context;

        public BTLookupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                return await _context.ProjectPriorities.ToListAsync();
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public Task<List<TicketPriority>> GetTicketPrioritiesAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<List<TicketStatus>> GetTicketStatusAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<List<TicketType>> GetTicketTypesAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}

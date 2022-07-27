﻿using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTProjectService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }

        //CRUD - create
        public async Task AddNewProjectAsync(Project project)
        {
            await _context.AddAsync(project);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            BTUser currentPM = await GetProjectManagerAsync(projectId);

            //Remove current PM if necessary
            if (currentPM != null)
            {
                try
                {
                    await RemoveProjectManagerAsync(projectId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing current PM. - Error: {ex.Message}");
                    return false;
                }
            }

            //Add the new PM
            try
            {
                await AddUserToProjectAsync(userId, projectId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**ERROR** - Adding new PM to project error. --> {ex.Message}");
                return false;
            }

        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            //The project knows about BTUser - so have to add user as type BTUser in order to add to
            //its' members - see try block below
            BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                //reason to use FirstOrDefaultAsync rather than find - get back an empty object you can test -
                //otherwise you do not get an object back
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                if (!await IsUserOnProjectAsync(userId, projectId))
                {
                    try
                    {
                        project.Members.Add(user);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    catch (Exception)
                    {
                        //something bad happens - here's a way to continue running; lets you see stack trace
                        //in a safe way in the proper place - let's users see it or not see it, you decide.
                        throw;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        //CRUD - Archive (Delete)
        public async Task ArchiveProjectAsync(Project project)
        {
            project.Archived = true;
            _context.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            List<BTUser> developers = await GetProjectMembersByRoleAsync(projectId, Roles.Developer.ToString());
            List<BTUser> submitters = await GetProjectMembersByRoleAsync(projectId, Roles.Submitter.ToString());
            List<BTUser> admins = await GetProjectMembersByRoleAsync(projectId, Roles.Admin.ToString());

            List<BTUser> teamMembers = developers.Concat(submitters).Concat(admins).ToList();

            return teamMembers;
        }

        public async Task<List<Project>> GetAllProjectsByCompany(int companyId)
        {
            List<Project> projects = new();

            return projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == false)
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

        public async Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName)
        {
            List<Project> projects = await GetAllProjectsByCompany(companyId);

            int priorityId = await LookupProjectPriorityId(priorityName);

            return projects.Where(p => p.ProjectPriorityId == priorityId).ToList();

        }


        public async Task<List<Project>> GetArchivedProjectsByCompany(int companyId)
        {
            List<Project> projects = await GetAllProjectsByCompany(companyId);

            return projects.Where(p => p.Archived == true).ToList();

        }

        //EXAMPLE
        public Task<List<BTUser>> GetDevelopersOnProjectAsync(int projectId)
        {
            throw new System.NotImplementedException();
        }

        //CRUD - Read
        public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            Project project = await _context.Projects
                                            .Include(p => p.Tickets)
                                            .Include(p => p.Members)
                                            .Include(p => p.ProjectPriority)
                                            .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

            return project;
        }

        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            Project project = await _context.Projects.Include(p => p.Members)
                                                     .FirstOrDefaultAsync(p => p.Id == projectId);

            foreach (BTUser member in project?.Members)
            {
                if (await _rolesService.IsUserInRoleAsync(member, Roles.ProjectManager.ToString()))
                {
                    return member;
                }
            }
            return null;
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            Project project = await _context.Projects.Include(p => p.Members)
                                                     .FirstOrDefaultAsync(p => p.Id == projectId);


            List<BTUser> members = new();

            foreach (BTUser user in project.Members)
            {
                if (await _rolesService.IsUserInRoleAsync(user, role))
                {
                    members.Add(user);
                }
            }
            return members;

        }

        //EXAMPLE
        public Task<List<BTUser>> GetSubmittersOnProjectAsync(int projectId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            try
            {
                //From the user table we produce a list of projects
                List<Project> userProjects = (await _context.Users
                    .Include(u => u.Projects)
                        .ThenInclude(p => p.Company)
                    .Include(u => u.Projects)
                        .ThenInclude(p => p.Members)
                    .Include(p => p.Projects)
                        .ThenInclude(t => t.Tickets)
                    .Include(u => u.Projects)
                        .ThenInclude(t => t.Tickets)
                            .ThenInclude(t => t.DeveloperUser)
                    .Include(u => u.Projects)
                        .ThenInclude(t => t.Tickets)
                            .ThenInclude(t => t.OwnerUser)
                    .Include(u => u.Projects)
                        .ThenInclude(t => t.Tickets)
                            .ThenInclude(t => t.TicketPriority)
                    .Include(u => u.Projects)
                        .ThenInclude(t => t.Tickets)
                            .ThenInclude(t => t.TicketStatus)
                    .Include(u => u.Projects)
                        .ThenInclude(t => t.Tickets)
                            .ThenInclude(t => t.TicketType)
                    .FirstOrDefaultAsync(u => u.Id == userId)).Projects.ToList();

                return userProjects;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** - Error getting user projects list! - {ex.Message}");
                throw;
            }
        }

        public async Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            List<BTUser> users = await _context.Users.Where(u => u.Projects.All(p => p.Id != projectId)).ToListAsync();

            return users.Where(u => u.CompanyId == companyId).ToList();
        }

        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            Project project = await _context.Projects.Include(p => p.Members)
                                                     .FirstOrDefaultAsync(p => p.Id == projectId);

            bool result = false;

            if (project != null)
            {
                result = project.Members.Any(m => m.Id == userId);
            }

            return result;
        }

        public async Task<int> LookupProjectPriorityId(string priorityName)
        {
            int priorityId = (await _context.ProjectPriorities.FirstOrDefaultAsync(p => p.Name == priorityName)).Id;

            return priorityId;
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            Project project = await _context.Projects.Include(p => p.Members)
                                                     .FirstOrDefaultAsync(p => p.Id == projectId);

            try
            {
                foreach (BTUser members in project?.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(members, Roles.ProjectManager.ToString()))
                    {
                        await RemoveUserFromProjectAsync(members.Id, project.Id);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                try
                {
                    if (await IsUserOnProjectAsync(userId, projectId))
                    {
                        project.Members.Remove(user);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception)
                {
                    throw;
                }

            }
            catch (Exception ex)
            {
                //Thrown onto the stack - custom error message to be specific
                Console.WriteLine($"****ERROR**** - Error removing user from project! - {ex.Message}");
            }
        }

        public async Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
        {
            try
            {
                List<BTUser> members = await GetProjectMembersByRoleAsync(projectId, role);
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser btUser in members)
                {
                    try
                    {
                        project.Members.Remove(btUser);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** - Error removing users from project! --> {ex.Message}");
                throw;
            }
        }

        //CRUD - Update
        public async Task UpdateProjectAsync(Project project)
        {
            _context.Update(project);
            await _context.SaveChangesAsync();
        }
    }
}

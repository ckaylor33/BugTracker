﻿using System;
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
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models.ViewModels;

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTLookupService _lookupService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTFileService _fileService;
        private readonly IBTTicketHistoryService _historyService;


        public TicketsController(UserManager<BTUser> userManager,
                                 IBTProjectService projectService,
                                 IBTLookupService lookupService,
                                 IBTTicketService ticketService,
                                 IBTCompanyInfoService companyInfoService,
                                 IBTFileService fileService,
                                 IBTTicketHistoryService historyService
                                 )
        {
            _userManager = userManager;
            _projectService = projectService;
            _lookupService = lookupService;
            _ticketService = ticketService;
            _companyInfoService = companyInfoService;
            _fileService = fileService;
            _historyService = historyService;
        }

        // GET: MyTickets
        public async Task<IActionResult> MyTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            string userId = _userManager.GetUserId(User);

            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(userId, companyId);

            return View(tickets);
        }

        //GET: AllTickets
        public async Task<IActionResult> AllTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);

            return View(tickets);

        }

        //GET: ArchivedTickets
        public async Task<IActionResult> ArchivedTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;


            List<Ticket> tickets = await _ticketService.GetArchivedTicketsAsync(companyId);


            return View(tickets);
        }

        //GET: UnassignedTickets
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            string btUserId = _userManager.GetUserId(User);

            List<Ticket> tickets = (await _ticketService.GetUnassignedTicketsAsync(companyId)).Where(t => t.Archived == false || t.ArchivedByProject == false).ToList();

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                return View(tickets);
            }
            else
            {
                List<Ticket> pmTickets = new();

                foreach (Ticket ticket in tickets)
                {
                    if (await _projectService.IsAssignedProjectManagerAsync(btUserId, ticket.ProjectId))
                    {
                        pmTickets.Add(ticket);
                    }
                }

                return View(pmTickets);
            }
        }

        //GET: AssignDeveloper
        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignDeveloper(int? id)
        {
            AssignDeveloperViewModel model = new();

            if (id == null)
            {
                return NotFound();
            }

            model.Ticket = await _ticketService.GetTicketByIdAsync(id.Value);
            model.Developers = new SelectList(await _projectService.GetProjectMembersByRoleAsync(model.Ticket.ProjectId, nameof(Roles.Developer)), "Id", "FullName");


            return View(model);
        }

        //POST: AssignDeveloper
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperViewModel model)
        {
            if (model.DeveloperId != null)
            {
                BTUser btUser = await _userManager.GetUserAsync(User);
                //Old Ticket
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket.Id);

                try
                {
                    await _ticketService.AssignTicketAsync(model.Ticket.Id, model.DeveloperId);
                }
                catch (Exception)
                {

                    throw;
                }
                //New Ticket
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket.Id);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, btUser.Id);

                return RedirectToAction(nameof(Details), new { id = model.Ticket.Id });
            }

            return RedirectToAction(nameof(AssignDeveloper), new { id = model.Ticket.Id });

        }

        //GET: UrgentTickets
        [HttpGet]
        public async Task<IActionResult> UrgentTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Ticket> tickets = (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).ToList();

            return View(tickets);
        }

        //GET: HighPriorityTickets
        [HttpGet]
        public async Task<IActionResult> HighPriorityTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Ticket> tickets = (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).ToList();

            return View(tickets);
        }

        //GET: HighPriorityTickets
        [HttpGet]
        public async Task<IActionResult> MediumPriorityTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Ticket> tickets = (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).ToList();

            return View(tickets);
        }

        //GET: HighPriorityTickets
        [HttpGet]
        public async Task<IActionResult> LowPriorityTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Ticket> tickets = (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).ToList();

            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            try
            {
                List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);

                return View(tickets);
            }
            catch (Exception)
            {

                throw;
            }
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            BTUser btUser = await _userManager.GetUserAsync(User);

            int companyId = User.Identity.GetCompanyId().Value;

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(btUser.Id), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            BTUser btUser = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                try
                {
                    ticket.Created = DateTimeOffset.Now;
                    ticket.OwnerUserId = btUser.Id;
                    ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync(nameof(BTTicketStatus.New))).Value;

                    await _ticketService.AddNewTicketAsync(ticket);

                    Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                    await _historyService.AddHistoryAsync(null, newTicket, btUser.Id);

                    //TODO: Ticket Notification
                }
                catch (Exception)
                {
                    throw;
                }

                return RedirectToAction(nameof(Details), new { id = ticket.Id });
            }

            if (User.IsInRole(nameof(Roles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyAsync(btUser.CompanyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(btUser.Id), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            return View(nameof(Create));
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                BTUser btUser = await _userManager.GetUserAsync(User);
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);

                try
                {
                    ticket.Updated = DateTimeOffset.Now;
                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, btUser.Id);

                return RedirectToAction(nameof(Details), new { id = ticket.Id });
            }
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                try
                {
                    ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                    ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                    ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;

                    ticketAttachment.Created = DateTimeOffset.Now;
                    ticketAttachment.UserId = _userManager.GetUserId(User);

                    await _ticketService.AddTicketAttachmentAsync(ticketAttachment);

                    //Add history
                    await _historyService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.UserId);
                }
                catch (Exception)
                {

                    throw;
                }

                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction(nameof(Details), new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id, TicketId, Comment")] TicketComment ticketComment)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    ticketComment.UserId = _userManager.GetUserId(User);
                    ticketComment.Created = DateTimeOffset.Now;

                    await _ticketService.AddTicketCommentAsync(ticketComment);

                    //Add history
                    await _historyService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.UserId);
                }
                catch (Exception)
                {

                    throw;
                }
            }

            return RedirectToAction("Details", new { id = ticketComment.TicketId });
        }

        // GET: Tickets/Archive/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("Archive")]
        [Authorize(Roles = "Admin, ProjectManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            await _ticketService.ArchiveTicketAsync(ticket);
            return RedirectToAction(nameof(AllTickets));
        }

        //POST: ResolveTicket
        [HttpPost]
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> ResolveTicket(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            BTUser btUser = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {

                //Old Ticket
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(id);

                try
                {
                    await _ticketService.ResolveTicketAsync(ticket);
                }
                catch (Exception)
                {

                    throw;
                }

                //New Ticket
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(id);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, btUser.Id);

                return RedirectToAction(nameof(Details), new { id = id });

            }

            return View(ticket);

        }

        //POST: ResolveTicket
        [HttpPost]
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> DevelopTicket(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            BTUser btUser = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {

                //Old Ticket
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(id);

                try
                {
                    await _ticketService.DevelopTicketAsync(ticket);
                }
                catch (Exception)
                {

                    throw;
                }

                //New Ticket
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(id);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, btUser.Id);

                return RedirectToAction(nameof(Details), new { id = id });

            }

            return View(ticket);

        }

        // GET: Tickets/Restore/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Restore/5
        [HttpPost, ActionName("Restore")]
        [Authorize(Roles = "Admin, ProjectManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
            await _ticketService.RestoreTicketAsync(ticket);
            return RedirectToAction(nameof(AllTickets));
        }

        private async Task<bool> TicketExists(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            return (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).Any(t => t.Id == id);
        }
    }
}

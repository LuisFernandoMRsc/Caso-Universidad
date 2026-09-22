using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusConnect.Web.Data;
using CampusConnect.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;

namespace CampusConnect.Web.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Student)
                .Include(t => t.AssignedSupport)
                .Include(t => t.Resource)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Student)
                .Include(t => t.AssignedSupport)
                .Include(t => t.Resource)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .Include(t => t.Evidences)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null) return NotFound();

            ViewBag.Technicians = await _context.Users
                .Where(u => u.Role == "Soporte" || u.Role == "Administrador")
                .ToListAsync();

            return View(ticket);
        }

        // POST: Tickets/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, int? assignedSupportId)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.Status = status;
            if (assignedSupportId.HasValue)
            {
                ticket.AssignedSupportId = assignedSupportId.Value;
            }
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Tickets/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int ticketId, string commentText)
        {
            if (!string.IsNullOrWhiteSpace(commentText))
            {
                // Usuario administrativo o de soporte por defecto (admin)
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Administrador") 
                                ?? await _context.Users.FirstAsync();

                var comment = new TicketComment
                {
                    TicketId = ticketId,
                    UserId = adminUser.Id,
                    CommentText = commentText,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TicketComments.Add(comment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = ticketId });
        }
    }
}

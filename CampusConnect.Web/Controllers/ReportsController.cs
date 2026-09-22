using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusConnect.Web.Data;
using Microsoft.AspNetCore.Authorization;

namespace CampusConnect.Web.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? status, string? priority, string? type)
        {
            var query = _context.Tickets
                .Include(t => t.Student)
                .Include(t => t.AssignedSupport)
                .Include(t => t.Resource)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }
            if (!string.IsNullOrEmpty(priority))
            {
                query = query.Where(t => t.Priority == priority);
            }
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(t => t.Type == type);
            }

            var tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

            // Métricas Consolidadas para el reporte
            ViewBag.TotalReceived = await _context.Tickets.CountAsync();
            ViewBag.TotalPending = await _context.Tickets.CountAsync(t => t.Status == "Pendiente");
            ViewBag.TotalInProgress = await _context.Tickets.CountAsync(t => t.Status == "En Proceso");
            ViewBag.TotalResolved = await _context.Tickets.CountAsync(t => t.Status == "Resuelta");
            ViewBag.TotalClosed = await _context.Tickets.CountAsync(t => t.Status == "Cerrada");

            // Agrupaciones
            ViewBag.TicketsByType = await _context.Tickets
                .GroupBy(t => t.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Type, g => g.Count);

            ViewBag.TicketsByPriority = await _context.Tickets
                .GroupBy(t => t.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Priority, g => g.Count);

            ViewBag.SelectedStatus = status;
            ViewBag.SelectedPriority = priority;
            ViewBag.SelectedType = type;

            return View(tickets);
        }
    }
}

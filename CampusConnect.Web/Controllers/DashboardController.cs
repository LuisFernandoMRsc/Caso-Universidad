using Microsoft.AspNetCore.Mvc;
using CampusConnect.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CampusConnect.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Métricas básicas para el Dashboard
            ViewBag.TotalTickets = await _context.Tickets.CountAsync();
            ViewBag.PendingTickets = await _context.Tickets.CountAsync(t => t.Status == "Pendiente");
            ViewBag.ResolvedTickets = await _context.Tickets.CountAsync(t => t.Status == "Resuelta");
            ViewBag.ClosedTickets = await _context.Tickets.CountAsync(t => t.Status == "Cerrada");

            return View();
        }
    }
}

using CampusConnect.Api.Data;
using CampusConnect.Api.DTOs;
using CampusConnect.Api.Models.Enums;
using CampusConnect.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusConnect.Api.Services.Implementations;

public class ReportesService : IReportesService
{
    private readonly ApplicationDbContext _context;
    private readonly PdfReportService _pdfReportService;
    private readonly ExcelReportService _excelReportService;

    public ReportesService(
        ApplicationDbContext context,
        PdfReportService pdfReportService,
        ExcelReportService excelReportService)
    {
        _context = context;
        _pdfReportService = pdfReportService;
        _excelReportService = excelReportService;
    }

    public async Task<DashboardMetricsDto> GetDashboardMetricsAsync()
    {
        var total = await _context.Solicitudes.CountAsync();

        var pendientes = await _context.Solicitudes.CountAsync(s =>
            s.Estado == EstadoSolicitud.REGISTRADA ||
            s.Estado == EstadoSolicitud.EN_EVALUACION ||
            s.Estado == EstadoSolicitud.ASIGNADA ||
            s.Estado == EstadoSolicitud.EN_PROCESO);

        var resueltas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitud.RESUELTA);
        var cerradas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitud.CERRADA);
        var rechazadas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitud.RECHAZADA);

        // Horas de resolución
        var conResolucion = await _context.Solicitudes
            .Where(s => s.FechaResolucion.HasValue)
            .Select(s => new { s.FechaCreacion, FechaResolucion = s.FechaResolucion!.Value })
            .ToListAsync();

        double avgResolucion = 0;
        if (conResolucion.Count > 0)
        {
            var totalHours = conResolucion.Sum(s => (s.FechaResolucion - s.FechaCreacion).TotalHours);
            avgResolucion = Math.Round(totalHours / conResolucion.Count, 1);
        }

        // Horas de asignación
        var conAsignacion = await _context.Solicitudes
            .Where(s => s.FechaAsignacion.HasValue)
            .Select(s => new { s.FechaCreacion, FechaAsignacion = s.FechaAsignacion!.Value })
            .ToListAsync();

        double avgAsignacion = 0;
        if (conAsignacion.Count > 0)
        {
            var totalHours = conAsignacion.Sum(s => (s.FechaAsignacion - s.FechaCreacion).TotalHours);
            avgAsignacion = Math.Round(totalHours / conAsignacion.Count, 1);
        }

        double efectividad = total > 0 ? Math.Round(((double)(resueltas + cerradas) / total) * 100, 1) : 0;

        return new DashboardMetricsDto
        {
            TotalSolicitudes = total,
            Pendientes = pendientes,
            Resueltas = resueltas,
            Cerradas = cerradas,
            Rechazadas = rechazadas,
            TiempoPromedioResolucionHoras = avgResolucion,
            TiempoPromedioAsignacionHoras = avgAsignacion,
            TasaEfectividad = efectividad
        };
    }

    public async Task<IEnumerable<CategoriaReporteDto>> GetSolicitudesPorTipoAsync()
    {
        var totalGlobal = await _context.Solicitudes.CountAsync();
        var categorias = await _context.Categorias
            .Include(c => c.Solicitudes)
            .ToListAsync();

        return categorias.Select(c =>
        {
            var cant = c.Solicitudes.Count;
            var pend = c.Solicitudes.Count(s =>
                s.Estado == EstadoSolicitud.REGISTRADA ||
                s.Estado == EstadoSolicitud.EN_EVALUACION ||
                s.Estado == EstadoSolicitud.ASIGNADA ||
                s.Estado == EstadoSolicitud.EN_PROCESO);
            var res = c.Solicitudes.Count(s => s.Estado == EstadoSolicitud.RESUELTA || s.Estado == EstadoSolicitud.CERRADA);
            var pct = totalGlobal > 0 ? Math.Round(((double)cant / totalGlobal) * 100, 1) : 0;

            return new CategoriaReporteDto
            {
                CategoriaId = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Icono = c.Icono,
                Color = c.Color,
                Cantidad = cant,
                Pendientes = pend,
                Resueltas = res,
                Porcentaje = pct
            };
        });
    }

    public async Task<IEnumerable<PrioridadReporteDto>> GetSolicitudesPorPrioridadAsync()
    {
        var totalGlobal = await _context.Solicitudes.CountAsync();
        var grupos = await _context.Solicitudes
            .GroupBy(s => s.Prioridad)
            .Select(g => new { Prioridad = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        return grupos.Select(g => new PrioridadReporteDto
        {
            Prioridad = g.Prioridad,
            Cantidad = g.Cantidad,
            Porcentaje = totalGlobal > 0 ? Math.Round(((double)g.Cantidad / totalGlobal) * 100, 1) : 0
        });
    }

    public async Task<IEnumerable<ResponsableReporteDto>> GetDesempenoResponsablesAsync()
    {
        var responsables = await _context.Usuarios
            .Where(u => u.Activo && (u.Rol == RolUsuario.TECNICO || u.Rol == RolUsuario.ADMINISTRATIVO || u.Rol == RolUsuario.ADMIN))
            .Include(u => u.Asignaciones)
                .ThenInclude(a => a.Solicitud)
            .ToListAsync();

        return responsables.Select(r =>
        {
            var totalAsignadas = r.Asignaciones.Count;
            var resueltas = r.Asignaciones.Count(a => a.Solicitud != null && (a.Solicitud.Estado == EstadoSolicitud.RESUELTA || a.Solicitud.Estado == EstadoSolicitud.CERRADA));
            var pendientes = totalAsignadas - resueltas;

            var resueltasConTiempo = r.Asignaciones
                .Where(a => a.Solicitud != null && a.Solicitud.FechaResolucion.HasValue)
                .Select(a => (a.Solicitud!.FechaResolucion!.Value - a.Solicitud.FechaCreacion).TotalHours)
                .ToList();

            double avgHours = resueltasConTiempo.Count > 0 ? Math.Round(resueltasConTiempo.Average(), 1) : 0;

            return new ResponsableReporteDto
            {
                Id = r.Id,
                NombreCompleto = r.NombreCompleto,
                Email = r.Email,
                Rol = r.Rol,
                Telefono = r.Telefono,
                TotalAsignadas = totalAsignadas,
                Pendientes = pendientes,
                Resueltas = resueltas,
                TiempoPromedioHoras = avgHours
            };
        });
    }

    public async Task<byte[]> GeneratePdfReportAsync()
    {
        var metrics = await GetDashboardMetricsAsync();
        var porTipo = await GetSolicitudesPorTipoAsync();
        var porPrioridad = await GetSolicitudesPorPrioridadAsync();
        var responsables = await GetDesempenoResponsablesAsync();

        return await _pdfReportService.GeneratePdfAsync(metrics, porTipo, porPrioridad, responsables);
    }

    public async Task<byte[]> GenerateExcelReportAsync()
    {
        var metrics = await GetDashboardMetricsAsync();
        var porTipo = await GetSolicitudesPorTipoAsync();
        var porPrioridad = await GetSolicitudesPorPrioridadAsync();

        return await _excelReportService.GenerateExcelAsync(metrics, porTipo, porPrioridad);
    }
}

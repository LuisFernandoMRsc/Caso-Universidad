using ClosedXML.Excel;
using CampusConnect.Api.Data;
using CampusConnect.Api.DTOs;
using CampusConnect.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampusConnect.Api.Services.Implementations;

public class ExcelReportService
{
    private readonly ApplicationDbContext _context;

    public ExcelReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerateExcelAsync(
        DashboardMetricsDto metrics,
        IEnumerable<CategoriaReporteDto> categorias,
        IEnumerable<PrioridadReporteDto> prioridades)
    {
        using var workbook = new XLWorkbook();

        // ================= HOJA 1: RESUMEN Y MÉTRICAS =================
        var wsResumen = workbook.Worksheets.Add("Resumen Ejecutivo");

        wsResumen.Cell(1, 1).Value = "CAMPUS CONNECT - INFORME CONSOLIDADO DE GESTIÓN";
        wsResumen.Range(1, 1, 1, 5).Merge().Style
            .Font.SetBold().Font.SetFontSize(14)
            .Font.SetFontColor(XLColor.White)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#1A237E"))
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        wsResumen.Cell(3, 1).Value = "Métrica Institucional";
        wsResumen.Cell(3, 2).Value = "Valor";
        wsResumen.Range(3, 1, 3, 2).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.FromHtml("#E8EAF6"));

        wsResumen.Cell(4, 1).Value = "Total Solicitudes Recibidas";
        wsResumen.Cell(4, 2).Value = metrics.TotalSolicitudes;

        wsResumen.Cell(5, 1).Value = "Solicitudes Pendientes de Atención";
        wsResumen.Cell(5, 2).Value = metrics.Pendientes;

        wsResumen.Cell(6, 1).Value = "Solicitudes Resueltas";
        wsResumen.Cell(6, 2).Value = metrics.Resueltas;

        wsResumen.Cell(7, 1).Value = "Solicitudes Cerradas";
        wsResumen.Cell(7, 2).Value = metrics.Cerradas;

        wsResumen.Cell(8, 1).Value = "Tiempo Promedio de Resolución (Horas)";
        wsResumen.Cell(8, 2).Value = metrics.TiempoPromedioResolucionHoras;

        wsResumen.Cell(9, 1).Value = "Tasa de Efectividad (%)";
        wsResumen.Cell(9, 2).Value = $"{metrics.TasaEfectividad}%";

        // Categorías
        int rowCat = 12;
        wsResumen.Cell(rowCat, 1).Value = "Categoría / Tipo";
        wsResumen.Cell(rowCat, 2).Value = "Total";
        wsResumen.Cell(rowCat, 3).Value = "Pendientes";
        wsResumen.Cell(rowCat, 4).Value = "Resueltas";
        wsResumen.Cell(rowCat, 5).Value = "Proporción";
        wsResumen.Range(rowCat, 1, rowCat, 5).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.FromHtml("#E8EAF6"));

        foreach (var c in categorias)
        {
            rowCat++;
            wsResumen.Cell(rowCat, 1).Value = c.Nombre;
            wsResumen.Cell(rowCat, 2).Value = c.Cantidad;
            wsResumen.Cell(rowCat, 3).Value = c.Pendientes;
            wsResumen.Cell(rowCat, 4).Value = c.Resueltas;
            wsResumen.Cell(rowCat, 5).Value = $"{c.Porcentaje}%";
        }

        // Prioridad
        int rowPri = rowCat + 3;
        wsResumen.Cell(rowPri, 1).Value = "Nivel de Prioridad";
        wsResumen.Cell(rowPri, 2).Value = "Cantidad";
        wsResumen.Cell(rowPri, 5).Value = "Proporción";
        wsResumen.Range(rowPri, 1, rowPri, 5).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.FromHtml("#E8EAF6"));

        foreach (var p in prioridades)
        {
            rowPri++;
            wsResumen.Cell(rowPri, 1).Value = $"Prioridad {p.Prioridad}";
            wsResumen.Cell(rowPri, 2).Value = p.Cantidad;
            wsResumen.Cell(rowPri, 5).Value = $"{p.Porcentaje}%";
        }

        wsResumen.Columns().AdjustToContents();

        // ================= HOJA 2: DETALLE SOLICITUDES =================
        var wsDetalle = workbook.Worksheets.Add("Detalle Solicitudes");

        var headers = new[]
        {
            "Código Ticket", "Título", "Estudiante", "Categoría", "Recurso",
            "Ubicación Detallada", "Prioridad", "Estado", "Canal",
            "Responsable Asignado", "Fecha Creación", "Fecha Resolución"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            wsDetalle.Cell(1, i + 1).Value = headers[i];
        }

        wsDetalle.Range(1, 1, 1, headers.Length).Style
            .Font.SetBold().Font.SetFontColor(XLColor.White)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#0D47A1"));

        var solicitudes = await _context.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Recurso)
            .Include(s => s.Estudiante)
            .Include(s => s.Asignaciones.Where(a => a.Activo))
                .ThenInclude(a => a.Responsable)
            .OrderByDescending(s => s.FechaCreacion)
            .ToListAsync();

        int rowIdx = 2;
        foreach (var s in solicitudes)
        {
            var resp = s.Asignaciones.FirstOrDefault()?.Responsable?.NombreCompleto ?? "Sin asignar";

            wsDetalle.Cell(rowIdx, 1).Value = s.TicketCode;
            wsDetalle.Cell(rowIdx, 2).Value = s.Titulo;
            wsDetalle.Cell(rowIdx, 3).Value = s.Estudiante?.NombreCompleto ?? "Desconocido";
            wsDetalle.Cell(rowIdx, 4).Value = s.Categoria?.Nombre ?? "N/A";
            wsDetalle.Cell(rowIdx, 5).Value = s.Recurso != null ? $"{s.Recurso.CodigoInventario} - {s.Recurso.Nombre}" : "N/A";
            wsDetalle.Cell(rowIdx, 6).Value = s.UbicacionDetallada;
            wsDetalle.Cell(rowIdx, 7).Value = s.Prioridad.ToString();
            wsDetalle.Cell(rowIdx, 8).Value = s.Estado.ToString();
            wsDetalle.Cell(rowIdx, 9).Value = s.CanalOrigen.ToString();
            wsDetalle.Cell(rowIdx, 10).Value = resp;
            wsDetalle.Cell(rowIdx, 11).Value = s.FechaCreacion.ToString("yyyy-MM-dd HH:mm");
            wsDetalle.Cell(rowIdx, 12).Value = s.FechaResolucion?.ToString("yyyy-MM-dd HH:mm") ?? "Pendiente";

            rowIdx++;
        }

        wsDetalle.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}

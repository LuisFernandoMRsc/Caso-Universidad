using CampusConnect.Api.Data;
using CampusConnect.Api.DTOs;
using CampusConnect.Api.Models.Entities;
using CampusConnect.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CampusConnect.Api.Services.Implementations;

public class PdfReportService
{
    private readonly ApplicationDbContext _context;

    public PdfReportService(ApplicationDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.ThrowOnMissingFontFamilies = false;
    }

    public async Task<byte[]> GeneratePdfAsync(
        DashboardMetricsDto metrics,
        IEnumerable<CategoriaReporteDto> categorias,
        IEnumerable<PrioridadReporteDto> prioridades,
        IEnumerable<ResponsableReporteDto> responsables)
    {
        var solicitudes = await _context.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Estudiante)
            .Include(s => s.Asignaciones.Where(a => a.Activo))
                .ThenInclude(a => a.Responsable)
            .OrderByDescending(s => s.FechaCreacion)
            .Take(15)
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                // ================= HEADER =================
                page.Header().Column(col =>
                {
                    col.Item().Background("#1A237E").Padding(15).Row(row =>
                    {
                        row.RelativeItem().Column(titleCol =>
                        {
                            titleCol.Item().Text("CAMPUS CONNECT").Bold().FontSize(18).FontColor(Colors.White);
                            titleCol.Item().Text("SISTEMA CENTRALIZADO DE SOLICITUDES UNIVERSITARIAS").FontSize(9).FontColor("#E8EAF6");
                        });

                        row.ConstantItem(150).AlignRight().Column(dateCol =>
                        {
                            dateCol.Item().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(9).FontColor(Colors.White);
                            dateCol.Item().Text("Reporte Consolidado").FontSize(8).FontColor("#E8EAF6");
                        });
                    });

                    col.Item().PaddingTop(10).PaddingBottom(10).AlignCenter().Text("INFORME EJECUTIVO DE GESTIÓN Y REQUERIMIENTOS")
                        .Bold().FontSize(14).FontColor("#0D47A1");
                });

                // ================= CONTENT =================
                page.Content().PaddingVertical(10).Column(col =>
                {
                    // 1. Tarjetas de KPIs
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Background("#1E88E5").Padding(8).Column(c =>
                        {
                            c.Item().AlignCenter().Text(metrics.TotalSolicitudes.ToString()).Bold().FontSize(14).FontColor(Colors.White);
                            c.Item().AlignCenter().Text("Total Recibidas").FontSize(7.5f).FontColor(Colors.White);
                        });

                        row.ConstantItem(6);

                        row.RelativeItem().Background("#FB8C00").Padding(8).Column(c =>
                        {
                            c.Item().AlignCenter().Text(metrics.Pendientes.ToString()).Bold().FontSize(14).FontColor(Colors.White);
                            c.Item().AlignCenter().Text("Pendientes").FontSize(7.5f).FontColor(Colors.White);
                        });

                        row.ConstantItem(6);

                        row.RelativeItem().Background("#43A047").Padding(8).Column(c =>
                        {
                            c.Item().AlignCenter().Text(metrics.Resueltas.ToString()).Bold().FontSize(14).FontColor(Colors.White);
                            c.Item().AlignCenter().Text("Resueltas").FontSize(7.5f).FontColor(Colors.White);
                        });

                        row.ConstantItem(6);

                        row.RelativeItem().Background("#546E7A").Padding(8).Column(c =>
                        {
                            c.Item().AlignCenter().Text(metrics.Cerradas.ToString()).Bold().FontSize(14).FontColor(Colors.White);
                            c.Item().AlignCenter().Text("Cerradas").FontSize(7.5f).FontColor(Colors.White);
                        });

                        row.ConstantItem(6);

                        row.RelativeItem().Background("#8E24AA").Padding(8).Column(c =>
                        {
                            c.Item().AlignCenter().Text($"{metrics.TiempoPromedioResolucionHoras} hrs").Bold().FontSize(14).FontColor(Colors.White);
                            c.Item().AlignCenter().Text("T. Promedio").FontSize(7.5f).FontColor(Colors.White);
                        });
                    });

                    // 2. Tabla Distribución según Tipo
                    col.Item().PaddingTop(15).Text("1. Solicitudes según Tipo / Categoría").Bold().FontSize(11).FontColor("#1A237E");
                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background("#EEEEEE").Padding(4).Text("Categoría").Bold().FontSize(8.5f);
                            header.Cell().Background("#EEEEEE").Padding(4).AlignCenter().Text("Total").Bold().FontSize(8.5f);
                            header.Cell().Background("#EEEEEE").Padding(4).AlignCenter().Text("Pendientes").Bold().FontSize(8.5f);
                            header.Cell().Background("#EEEEEE").Padding(4).AlignCenter().Text("Resueltas").Bold().FontSize(8.5f);
                            header.Cell().Background("#EEEEEE").Padding(4).AlignCenter().Text("Proporción").Bold().FontSize(8.5f);
                        });

                        foreach (var cat in categorias)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor("#DDDDDD").Padding(4).Text(cat.Nombre).FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor("#DDDDDD").Padding(4).AlignCenter().Text(cat.Cantidad.ToString()).FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor("#DDDDDD").Padding(4).AlignCenter().Text(cat.Pendientes.ToString()).FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor("#DDDDDD").Padding(4).AlignCenter().Text(cat.Resueltas.ToString()).FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor("#DDDDDD").Padding(4).AlignCenter().Text($"{cat.Porcentaje}%").FontSize(8);
                        }
                    });

                    // 3. Distribución según Prioridad
                    col.Item().PaddingTop(15).Text("2. Solicitudes según Nivel de Prioridad").Bold().FontSize(11).FontColor("#1A237E");
                    col.Item().PaddingTop(4).Column(priCol =>
                    {
                        foreach (var p in prioridades)
                        {
                            priCol.Item().PaddingVertical(1).Text($"• Prioridad {p.Prioridad}: {p.Cantidad} requerimiento(s) ({p.Porcentaje}% del total)").FontSize(8.5f);
                        }
                    });

                    // 4. Desempeño de Responsables
                    col.Item().PaddingTop(15).Text("3. Desempeño de Responsables Asignados").Bold().FontSize(11).FontColor("#1A237E");
                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background("#EEEEEE").Padding(4).Text("Responsable").Bold().FontSize(8.5f);
                            header.Cell().Background("#EEEEEE").Padding(4).Text("Rol").Bold().FontSize(8.5f);
                            header.Cell().Background("#EEEEEE").Padding(4).AlignCenter().Text("Asignadas").Bold().FontSize(8.5f);
                            header.Cell().Background("#EEEEEE").Padding(4).AlignCenter().Text("Resueltas").Bold().FontSize(8.5f);
                            header.Cell().Background("#EEEEEE").Padding(4).AlignCenter().Text("T. Promedio").Bold().FontSize(8.5f);
                        });

                        foreach (var r in responsables)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor("#DDDDDD").Padding(4).Text(r.NombreCompleto).FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor("#DDDDDD").Padding(4).Text(r.Rol.ToString()).FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor("#DDDDDD").Padding(4).AlignCenter().Text(r.TotalAsignadas.ToString()).FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor("#DDDDDD").Padding(4).AlignCenter().Text(r.Resueltas.ToString()).FontSize(8);
                            table.Cell().BorderBottom(0.5f).BorderColor("#DDDDDD").Padding(4).AlignCenter().Text($"{r.TiempoPromedioHoras} hrs").FontSize(8);
                        }
                    });

                    // 5. Muestra de Solicitudes Registradas
                    col.Item().PaddingTop(15).Text("4. Requerimientos Recientes Centralizados").Bold().FontSize(11).FontColor("#1A237E");
                    col.Item().PaddingTop(4).Column(solCol =>
                    {
                        foreach (var s in solicitudes)
                        {
                            var resp = s.Asignaciones.FirstOrDefault()?.Responsable?.NombreCompleto ?? "Sin asignar";
                            solCol.Item().BorderBottom(0.5f).BorderColor("#EEEEEE").PaddingVertical(3).Column(itemCol =>
                            {
                                itemCol.Item().Row(r =>
                                {
                                    r.RelativeItem().Text($"[{s.TicketCode}] {s.Titulo}").Bold().FontSize(8.5f).FontColor("#0D47A1");
                                    r.ConstantItem(100).AlignRight().Text(s.Estado.ToString()).Bold().FontSize(8);
                                });
                                itemCol.Item().Text($"Estudiante: {s.Estudiante?.NombreCompleto} | Categ: {s.Categoria?.Nombre} | Prioridad: {s.Prioridad} | Responsable: {resp}").FontSize(7.5f).FontColor("#555555");
                            });
                        }
                    });
                });

                // ================= FOOTER =================
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                    x.Span(" | Campus Connect - Plataforma Universitaria Integrada");
                });
            });
        });

        return document.GeneratePdf();
    }
}

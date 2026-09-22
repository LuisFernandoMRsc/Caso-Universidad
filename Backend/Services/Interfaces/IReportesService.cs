using CampusConnect.Api.DTOs;
using CampusConnect.Api.Models.Entities;

namespace CampusConnect.Api.Services.Interfaces;

public interface IReportesService
{
    Task<DashboardMetricsDto> GetDashboardMetricsAsync();
    Task<IEnumerable<CategoriaReporteDto>> GetSolicitudesPorTipoAsync();
    Task<IEnumerable<PrioridadReporteDto>> GetSolicitudesPorPrioridadAsync();
    Task<IEnumerable<ResponsableReporteDto>> GetDesempenoResponsablesAsync();
    Task<byte[]> GeneratePdfReportAsync();
    Task<byte[]> GenerateExcelReportAsync();
}

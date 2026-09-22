using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CampusConnect.Api.DTOs;
using CampusConnect.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CampusConnect.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly IReportesService _reportesService;
    private readonly IConfiguration _configuration;

    public ReportesController(IReportesService reportesService, IConfiguration configuration)
    {
        _reportesService = reportesService;
        _configuration = configuration;
    }

    private bool ValidateDownloadToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;

        try
        {
            var secret = _configuration["Jwt:Secret"] ?? "campus_connect_super_secret_jwt_key_2026_universidad";
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var role = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role || x.Type == "role")?.Value;

            return role == "ADMIN" || role == "ADMINISTRATIVO" || role == "TECNICO";
        }
        catch
        {
            return false;
        }
    }

    [Authorize(Roles = "ADMIN,ADMINISTRATIVO,TECNICO")]
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardMetricsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        var metrics = await _reportesService.GetDashboardMetricsAsync();
        return Ok(ApiResponse<DashboardMetricsDto>.Ok(metrics, "Métricas consolidadas del dashboard obtenidas"));
    }

    [Authorize(Roles = "ADMIN,ADMINISTRATIVO,TECNICO")]
    [HttpGet("por-tipo")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoriaReporteDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPorTipo()
    {
        var data = await _reportesService.GetSolicitudesPorTipoAsync();
        return Ok(ApiResponse<IEnumerable<CategoriaReporteDto>>.Ok(data, "Solicitudes según tipo obtenidas"));
    }

    [Authorize(Roles = "ADMIN,ADMINISTRATIVO,TECNICO")]
    [HttpGet("por-prioridad")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PrioridadReporteDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPorPrioridad()
    {
        var data = await _reportesService.GetSolicitudesPorPrioridadAsync();
        return Ok(ApiResponse<IEnumerable<PrioridadReporteDto>>.Ok(data, "Solicitudes según prioridad obtenidas"));
    }

    [Authorize(Roles = "ADMIN,ADMINISTRATIVO,TECNICO")]
    [HttpGet("por-responsable")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ResponsableReporteDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPorResponsable()
    {
        var data = await _reportesService.GetDesempenoResponsablesAsync();
        return Ok(ApiResponse<IEnumerable<ResponsableReporteDto>>.Ok(data, "Desempeño de responsables obtenido"));
    }

    [HttpGet("exportar/pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] string? token)
    {
        // Puede autenticarse por Header Bearer o por query param ?token=
        var isAuthorized = User.Identity?.IsAuthenticated == true || ValidateDownloadToken(token);
        if (!isAuthorized)
        {
            return Unauthorized(ApiResponse<object>.Fail("Token de autorización requerido para la descarga"));
        }

        var bytes = await _reportesService.GeneratePdfReportAsync();
        var fileName = $"Reporte_CampusConnect_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
        return File(bytes, "application/pdf", fileName);
    }

    [HttpGet("exportar/excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] string? token)
    {
        var isAuthorized = User.Identity?.IsAuthenticated == true || ValidateDownloadToken(token);
        if (!isAuthorized)
        {
            return Unauthorized(ApiResponse<object>.Fail("Token de autorización requerido para la descarga"));
        }

        var bytes = await _reportesService.GenerateExcelReportAsync();
        var fileName = $"Reporte_CampusConnect_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}

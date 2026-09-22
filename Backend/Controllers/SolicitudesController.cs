using System.Security.Claims;
using CampusConnect.Api.DTOs;
using CampusConnect.Api.Models.Entities;
using CampusConnect.Api.Models.Enums;
using CampusConnect.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusConnect.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SolicitudesController : ControllerBase
{
    private readonly ISolicitudesService _solicitudesService;

    public SolicitudesController(ISolicitudesService solicitudesService)
    {
        _solicitudesService = solicitudesService;
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out int id) ? id : 0;
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<Solicitud>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromForm] CreateSolicitudDto dto)
    {
        var studentId = GetCurrentUserId();
        if (studentId == 0)
        {
            return Unauthorized(ApiResponse<object>.Fail("No autenticado"));
        }

        var solicitud = await _solicitudesService.CreateSolicitudAsync(dto, studentId);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<Solicitud>.Ok(solicitud, $"Solicitud creada con código {solicitud.TicketCode}"));
    }

    [HttpGet("mis-solicitudes")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Solicitud>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMisSolicitudes([FromQuery] EstadoSolicitud? estado, [FromQuery] int page = 1, [FromQuery] int limit = 20, [FromQuery] string? search = null)
    {
        var studentId = GetCurrentUserId();
        if (studentId == 0)
        {
            return Unauthorized(ApiResponse<object>.Fail("No autenticado"));
        }

        var filter = new SolicitudFilterDto
        {
            EstudianteId = studentId,
            Estado = estado,
            Page = page,
            Limit = limit,
            Search = search
        };

        var result = await _solicitudesService.GetSolicitudesAsync(filter);
        return Ok(ApiResponse<IEnumerable<Solicitud>>.Ok(result.Items, "Mis solicitudes obtenidas", new
        {
            result.Total,
            result.Page,
            result.Limit,
            result.TotalPages
        }));
    }

    [Authorize(Roles = "ADMIN,ADMINISTRATIVO,TECNICO")]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Solicitud>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] SolicitudFilterDto filter)
    {
        var result = await _solicitudesService.GetSolicitudesAsync(filter);
        return Ok(ApiResponse<IEnumerable<Solicitud>>.Ok(result.Items, "Solicitudes obtenidas", new
        {
            result.Total,
            result.Page,
            result.Limit,
            result.TotalPages
        }));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<Solicitud>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var solicitud = await _solicitudesService.GetSolicitudByIdAsync(id);
        if (solicitud == null)
        {
            return NotFound(ApiResponse<object>.Fail("Solicitud no encontrada"));
        }

        var currentUserId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        // Si es estudiante, verificar que sea el propietario
        if (userRole == RolUsuario.ESTUDIANTE.ToString() && solicitud.EstudianteId != currentUserId)
        {
            return Forbid();
        }

        return Ok(ApiResponse<Solicitud>.Ok(solicitud, "Detalle de la solicitud obtenido"));
    }

    [Authorize(Roles = "ADMIN,ADMINISTRATIVO,TECNICO")]
    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(typeof(ApiResponse<Solicitud>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDto dto)
    {
        var currentUserId = GetCurrentUserId();
        try
        {
            var updated = await _solicitudesService.CambiarEstadoAsync(id, dto.Estado, currentUserId, dto.Detalle);
            return Ok(ApiResponse<Solicitud>.Ok(updated, "Estado actualizado con éxito"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [Authorize(Roles = "ADMIN,ADMINISTRATIVO")]
    [HttpPost("{id:int}/asignar")]
    [ProducesResponseType(typeof(ApiResponse<Solicitud>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Asignar(int id, [FromBody] AsignarResponsableDto dto)
    {
        var currentUserId = GetCurrentUserId();
        try
        {
            var updated = await _solicitudesService.AsignarResponsableAsync(id, dto.ResponsableId, currentUserId, dto.Notas);
            return Ok(ApiResponse<Solicitud>.Ok(updated, "Responsable asignado correctamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [Authorize(Roles = "ADMIN,ADMINISTRATIVO")]
    [HttpPatch("{id:int}/prioridad")]
    [ProducesResponseType(typeof(ApiResponse<Solicitud>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CambiarPrioridad(int id, [FromBody] CambiarPrioridadDto dto)
    {
        var currentUserId = GetCurrentUserId();
        try
        {
            var updated = await _solicitudesService.CambiarPrioridadAsync(id, dto.Prioridad, currentUserId, dto.Motivo);
            return Ok(ApiResponse<Solicitud>.Ok(updated, "Prioridad actualizada"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("{id:int}/comentarios")]
    [ProducesResponseType(typeof(ApiResponse<ComentarioSolicitud>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AgregarComentario(int id, [FromBody] AgregarComentarioDto dto)
    {
        var currentUserId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        // Estudiantes no pueden enviar notas internas
        bool esInternoFinal = userRole == RolUsuario.ESTUDIANTE.ToString() ? false : dto.EsInterno;

        try
        {
            var comentario = await _solicitudesService.AgregarComentarioAsync(id, currentUserId, dto.Mensaje, esInternoFinal);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<ComentarioSolicitud>.Ok(comentario, "Comentario agregado"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpGet("{id:int}/trazabilidad")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<HistorialTrazabilidad>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrazabilidad(int id)
    {
        var trazabilidad = await _solicitudesService.GetTrazabilidadAsync(id);
        return Ok(ApiResponse<IEnumerable<HistorialTrazabilidad>>.Ok(trazabilidad, "Historial de trazabilidad obtenido"));
    }
}

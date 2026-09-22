using CampusConnect.Api.DTOs;
using CampusConnect.Api.Models.Entities;
using CampusConnect.Api.Models.Enums;

namespace CampusConnect.Api.Services.Interfaces;

public interface ISolicitudesService
{
    Task<Solicitud> CreateSolicitudAsync(CreateSolicitudDto dto, int estudianteId);
    Task<PagedResult<Solicitud>> GetSolicitudesAsync(SolicitudFilterDto filter);
    Task<Solicitud?> GetSolicitudByIdAsync(int id);
    Task<Solicitud> CambiarEstadoAsync(int id, EstadoSolicitud nuevoEstado, int usuarioId, string? detalle);
    Task<Solicitud> AsignarResponsableAsync(int id, int responsableId, int usuarioAsignadorId, string? notas);
    Task<Solicitud> CambiarPrioridadAsync(int id, PrioridadSolicitud nuevaPrioridad, int usuarioId, string? motivo);
    Task<ComentarioSolicitud> AgregarComentarioAsync(int solicitudId, int usuarioId, string mensaje, bool esInterno);
    Task<IEnumerable<HistorialTrazabilidad>> GetTrazabilidadAsync(int solicitudId);
}

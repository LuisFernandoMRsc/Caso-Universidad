using CampusConnect.Api.Data;
using CampusConnect.Api.DTOs;
using CampusConnect.Api.Models.Entities;
using CampusConnect.Api.Models.Enums;
using CampusConnect.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusConnect.Api.Services.Implementations;

public class SolicitudesService : ISolicitudesService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public SolicitudesService(ApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    private async Task<string> GenerateTicketCodeAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"SOL-{year}-";

        var lastTicket = await _context.Solicitudes
            .Where(s => s.TicketCode.StartsWith(prefix))
            .OrderByDescending(s => s.Id)
            .Select(s => s.TicketCode)
            .FirstOrDefaultAsync();

        int nextSeq = 1;
        if (!string.IsNullOrEmpty(lastTicket))
        {
            var parts = lastTicket.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int seq))
            {
                nextSeq = seq + 1;
            }
        }

        return $"{prefix}{nextSeq:D4}";
    }

    public async Task<Solicitud> CreateSolicitudAsync(CreateSolicitudDto dto, int estudianteId)
    {
        var ticketCode = await GenerateTicketCodeAsync();

        var solicitud = new Solicitud
        {
            TicketCode = ticketCode,
            Titulo = dto.Titulo.Trim(),
            Descripcion = dto.Descripcion.Trim(),
            UbicacionDetallada = dto.UbicacionDetallada.Trim(),
            CategoriaId = dto.CategoriaId,
            RecursoId = dto.RecursoId,
            Prioridad = dto.Prioridad,
            CanalOrigen = dto.CanalOrigen,
            Estado = EstadoSolicitud.REGISTRADA,
            EstudianteId = estudianteId,
            FechaCreacion = DateTime.UtcNow
        };

        await _context.Solicitudes.AddAsync(solicitud);
        await _context.SaveChangesAsync();

        // Guardar evidencias si fueron subidas
        if (dto.Evidencias != null && dto.Evidencias.Count > 0)
        {
            foreach (var file in dto.Evidencias)
            {
                if (file.Length > 0)
                {
                    var (relPath, originalName, mime, size) = await _fileStorage.SaveFileAsync(file);
                    var evidencia = new EvidenciaSolicitud
                    {
                        SolicitudId = solicitud.Id,
                        RutaArchivo = relPath,
                        NombreOriginal = originalName,
                        TipoMimetype = mime,
                        TamanoBytes = size,
                        FechaSubida = DateTime.UtcNow
                    };
                    await _context.Evidencias.AddAsync(evidencia);
                }
            }
            await _context.SaveChangesAsync();
        }

        // Registrar en historial de trazabilidad
        var historial = new HistorialTrazabilidad
        {
            SolicitudId = solicitud.Id,
            UsuarioId = estudianteId,
            Accion = TipoAccionTrazabilidad.CREACION,
            EstadoAnterior = null,
            EstadoNuevo = EstadoSolicitud.REGISTRADA,
            Detalle = $"Solicitud registrada con código único [{ticketCode}] a través de {dto.CanalOrigen}.",
            FechaRegistro = DateTime.UtcNow
        };

        await _context.HistorialTrazabilidad.AddAsync(historial);
        await _context.SaveChangesAsync();

        return (await GetSolicitudByIdAsync(solicitud.Id))!;
    }

    public async Task<PagedResult<Solicitud>> GetSolicitudesAsync(SolicitudFilterDto filter)
    {
        var page = Math.Max(1, filter.Page);
        var limit = Math.Max(1, filter.Limit);

        var query = _context.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Recurso)
            .Include(s => s.Estudiante)
            .Include(s => s.Evidencias)
            .Include(s => s.Asignaciones.Where(a => a.Activo))
                .ThenInclude(a => a.Responsable)
            .AsQueryable();

        if (filter.Estado.HasValue)
        {
            query = query.Where(s => s.Estado == filter.Estado.Value);
        }

        if (filter.Prioridad.HasValue)
        {
            query = query.Where(s => s.Prioridad == filter.Prioridad.Value);
        }

        if (filter.CategoriaId.HasValue)
        {
            query = query.Where(s => s.CategoriaId == filter.CategoriaId.Value);
        }

        if (filter.RecursoId.HasValue)
        {
            query = query.Where(s => s.RecursoId == filter.RecursoId.Value);
        }

        if (filter.EstudianteId.HasValue)
        {
            query = query.Where(s => s.EstudianteId == filter.EstudianteId.Value);
        }

        if (filter.ResponsableId.HasValue)
        {
            query = query.Where(s => s.Asignaciones.Any(a => a.ResponsableId == filter.ResponsableId.Value && a.Activo));
        }

        if (filter.FechaDesde.HasValue)
        {
            query = query.Where(s => s.FechaCreacion >= filter.FechaDesde.Value);
        }

        if (filter.FechaHasta.HasValue)
        {
            query = query.Where(s => s.FechaCreacion <= filter.FechaHasta.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(item =>
                item.TicketCode.ToLower().Contains(s) ||
                item.Titulo.ToLower().Contains(s) ||
                item.Descripcion.ToLower().Contains(s) ||
                item.UbicacionDetallada.ToLower().Contains(s) ||
                (item.Estudiante != null && item.Estudiante.NombreCompleto.ToLower().Contains(s)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(s => s.FechaCreacion)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return new PagedResult<Solicitud>
        {
            Items = items,
            Total = total,
            Page = page,
            Limit = limit
        };
    }

    public async Task<Solicitud?> GetSolicitudByIdAsync(int id)
    {
        return await _context.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Recurso)
            .Include(s => s.Estudiante)
            .Include(s => s.Evidencias)
            .Include(s => s.Asignaciones)
                .ThenInclude(a => a.Responsable)
            .Include(s => s.Historial.OrderBy(h => h.FechaRegistro))
                .ThenInclude(h => h.Usuario)
            .Include(s => s.Comentarios.OrderBy(c => c.FechaCreacion))
                .ThenInclude(c => c.Usuario)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Solicitud> CambiarEstadoAsync(int id, EstadoSolicitud nuevoEstado, int usuarioId, string? detalle)
    {
        var solicitud = await _context.Solicitudes.FirstOrDefaultAsync(s => s.Id == id);
        if (solicitud == null)
        {
            throw new KeyNotFoundException("Solicitud no encontrada.");
        }

        var estadoAnterior = solicitud.Estado;
        solicitud.Estado = nuevoEstado;

        if (nuevoEstado == EstadoSolicitud.RESUELTA && !solicitud.FechaResolucion.HasValue)
        {
            solicitud.FechaResolucion = DateTime.UtcNow;
        }

        if (nuevoEstado == EstadoSolicitud.CERRADA && !solicitud.FechaCierre.HasValue)
        {
            solicitud.FechaCierre = DateTime.UtcNow;
        }

        var historial = new HistorialTrazabilidad
        {
            SolicitudId = id,
            UsuarioId = usuarioId,
            Accion = TipoAccionTrazabilidad.CAMBIO_ESTADO,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = nuevoEstado,
            Detalle = detalle ?? $"Estado actualizado de {estadoAnterior} a {nuevoEstado}.",
            FechaRegistro = DateTime.UtcNow
        };

        await _context.HistorialTrazabilidad.AddAsync(historial);
        await _context.SaveChangesAsync();

        return (await GetSolicitudByIdAsync(id))!;
    }

    public async Task<Solicitud> AsignarResponsableAsync(int id, int responsableId, int usuarioAsignadorId, string? notas)
    {
        var solicitud = await _context.Solicitudes.FirstOrDefaultAsync(s => s.Id == id);
        if (solicitud == null)
        {
            throw new KeyNotFoundException("Solicitud no encontrada.");
        }

        var responsable = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == responsableId);
        if (responsable == null)
        {
            throw new KeyNotFoundException("El responsable seleccionado no existe.");
        }

        // Desactivar asignaciones activas anteriores
        var prevAsignaciones = await _context.Asignaciones
            .Where(a => a.SolicitudId == id && a.Activo)
            .ToListAsync();

        foreach (var a in prevAsignaciones)
        {
            a.Activo = false;
        }

        var nuevaAsignacion = new AsignacionResponsable
        {
            SolicitudId = id,
            ResponsableId = responsableId,
            Notas = notas,
            Activo = true,
            FechaAsignacion = DateTime.UtcNow
        };

        await _context.Asignaciones.AddAsync(nuevaAsignacion);

        var estadoAnterior = solicitud.Estado;
        if (solicitud.Estado == EstadoSolicitud.REGISTRADA || solicitud.Estado == EstadoSolicitud.EN_EVALUACION)
        {
            solicitud.Estado = EstadoSolicitud.ASIGNADA;
        }

        if (!solicitud.FechaAsignacion.HasValue)
        {
            solicitud.FechaAsignacion = DateTime.UtcNow;
        }

        var historial = new HistorialTrazabilidad
        {
            SolicitudId = id,
            UsuarioId = usuarioAsignadorId,
            Accion = TipoAccionTrazabilidad.ASIGNACION,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = solicitud.Estado,
            Detalle = $"Solicitud derivada y asignada a [{responsable.NombreCompleto} ({responsable.Rol})]. {(string.IsNullOrEmpty(notas) ? "" : "Notas: " + notas)}",
            FechaRegistro = DateTime.UtcNow
        };

        await _context.HistorialTrazabilidad.AddAsync(historial);
        await _context.SaveChangesAsync();

        return (await GetSolicitudByIdAsync(id))!;
    }

    public async Task<Solicitud> CambiarPrioridadAsync(int id, PrioridadSolicitud nuevaPrioridad, int usuarioId, string? motivo)
    {
        var solicitud = await _context.Solicitudes.FirstOrDefaultAsync(s => s.Id == id);
        if (solicitud == null)
        {
            throw new KeyNotFoundException("Solicitud no encontrada.");
        }

        var prioridadAnterior = solicitud.Prioridad;
        solicitud.Prioridad = nuevaPrioridad;

        var historial = new HistorialTrazabilidad
        {
            SolicitudId = id,
            UsuarioId = usuarioId,
            Accion = TipoAccionTrazabilidad.CAMBIO_PRIORIDAD,
            Detalle = $"Prioridad modificada de {prioridadAnterior} a {nuevaPrioridad}. {(string.IsNullOrEmpty(motivo) ? "" : "Motivo: " + motivo)}",
            FechaRegistro = DateTime.UtcNow
        };

        await _context.HistorialTrazabilidad.AddAsync(historial);
        await _context.SaveChangesAsync();

        return (await GetSolicitudByIdAsync(id))!;
    }

    public async Task<ComentarioSolicitud> AgregarComentarioAsync(int solicitudId, int usuarioId, string mensaje, bool esInterno)
    {
        var solicitud = await _context.Solicitudes.FirstOrDefaultAsync(s => s.Id == solicitudId);
        if (solicitud == null)
        {
            throw new KeyNotFoundException("Solicitud no encontrada.");
        }

        var comentario = new ComentarioSolicitud
        {
            SolicitudId = solicitudId,
            UsuarioId = usuarioId,
            Mensaje = mensaje.Trim(),
            EsInterno = esInterno,
            FechaCreacion = DateTime.UtcNow
        };

        await _context.Comentarios.AddAsync(comentario);
        await _context.SaveChangesAsync();

        return (await _context.Comentarios
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.Id == comentario.Id))!;
    }

    public async Task<IEnumerable<HistorialTrazabilidad>> GetTrazabilidadAsync(int solicitudId)
    {
        return await _context.HistorialTrazabilidad
            .Include(h => h.Usuario)
            .Where(h => h.SolicitudId == solicitudId)
            .OrderBy(h => h.FechaRegistro)
            .ToListAsync();
    }
}

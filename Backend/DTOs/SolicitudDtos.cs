using System.ComponentModel.DataAnnotations;
using CampusConnect.Api.Models.Enums;
using Microsoft.AspNetCore.Http;

namespace CampusConnect.Api.DTOs;

public class CreateSolicitudDto
{
    [Required]
    [MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string UbicacionDetallada { get; set; } = string.Empty;

    [Required]
    public int CategoriaId { get; set; }

    public int? RecursoId { get; set; }

    public PrioridadSolicitud Prioridad { get; set; } = PrioridadSolicitud.MEDIA;

    public CanalOrigen CanalOrigen { get; set; } = CanalOrigen.APP_MOVIL;

    public List<IFormFile>? Evidencias { get; set; }
}

public class SolicitudFilterDto
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
    public EstadoSolicitud? Estado { get; set; }
    public PrioridadSolicitud? Prioridad { get; set; }
    public int? CategoriaId { get; set; }
    public int? RecursoId { get; set; }
    public int? EstudianteId { get; set; }
    public int? ResponsableId { get; set; }
    public string? Search { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}

public class CambiarEstadoDto
{
    [Required]
    public EstadoSolicitud Estado { get; set; }
    public string? Detalle { get; set; }
}

public class AsignarResponsableDto
{
    [Required]
    public int ResponsableId { get; set; }
    public string? Notas { get; set; }
}

public class CambiarPrioridadDto
{
    [Required]
    public PrioridadSolicitud Prioridad { get; set; }
    public string? Motivo { get; set; }
}

public class AgregarComentarioDto
{
    [Required]
    public string Mensaje { get; set; } = string.Empty;
    public bool EsInterno { get; set; } = false;
}

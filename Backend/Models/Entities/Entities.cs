using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CampusConnect.Api.Models.Enums;

namespace CampusConnect.Api.Models.Entities;

[Table("usuarios")]
public class Usuario
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [MaxLength(50)]
    [Column("carnet_codigo")]
    public string? CarnetCodigo { get; set; }

    [Required]
    [MaxLength(150)]
    [Column("nombre_completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [EmailAddress]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [JsonIgnore]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [Column("rol")]
    public RolUsuario Rol { get; set; } = RolUsuario.ESTUDIANTE;

    [MaxLength(30)]
    [Column("telefono")]
    public string? Telefono { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [JsonIgnore]
    public ICollection<Solicitud> SolicitudesCreadas { get; set; } = new List<Solicitud>();

    [JsonIgnore]
    public ICollection<AsignacionResponsable> Asignaciones { get; set; } = new List<AsignacionResponsable>();

    [JsonIgnore]
    public ICollection<HistorialTrazabilidad> HistorialTrazabilidad { get; set; } = new List<HistorialTrazabilidad>();

    [JsonIgnore]
    public ICollection<ComentarioSolicitud> Comentarios { get; set; } = new List<ComentarioSolicitud>();
}

[Table("categorias_solicitud")]
public class CategoriaSolicitud
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [MaxLength(50)]
    [Column("icono")]
    public string? Icono { get; set; }

    [MaxLength(20)]
    [Column("color")]
    public string? Color { get; set; }

    [Column("activa")]
    public bool Activa { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
}

[Table("recursos_institucionales")]
public class RecursoInstitucional
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("codigo_inventario")]
    public string CodigoInventario { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("tipo")]
    public TipoRecurso Tipo { get; set; } = TipoRecurso.EQUIPO;

    [Required]
    [MaxLength(200)]
    [Column("ubicacion")]
    public string Ubicacion { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("estado")]
    public string Estado { get; set; } = "OPERATIVO";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
}

[Table("solicitudes")]
public class Solicitud
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    [Column("ticket_code")]
    public string TicketCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [Column("descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("ubicacion_detallada")]
    public string UbicacionDetallada { get; set; } = string.Empty;

    [Column("prioridad")]
    public PrioridadSolicitud Prioridad { get; set; } = PrioridadSolicitud.MEDIA;

    [Column("estado")]
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.REGISTRADA;

    [Column("canal_origen")]
    public CanalOrigen CanalOrigen { get; set; } = CanalOrigen.APP_MOVIL;

    [Column("estudiante_id")]
    public int EstudianteId { get; set; }
    public Usuario? Estudiante { get; set; }

    [Column("categoria_id")]
    public int CategoriaId { get; set; }
    public CategoriaSolicitud? Categoria { get; set; }

    [Column("recurso_id")]
    public int? RecursoId { get; set; }
    public RecursoInstitucional? Recurso { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [Column("fecha_asignacion")]
    public DateTime? FechaAsignacion { get; set; }

    [Column("fecha_resolucion")]
    public DateTime? FechaResolucion { get; set; }

    [Column("fecha_cierre")]
    public DateTime? FechaCierre { get; set; }

    public ICollection<EvidenciaSolicitud> Evidencias { get; set; } = new List<EvidenciaSolicitud>();
    public ICollection<AsignacionResponsable> Asignaciones { get; set; } = new List<AsignacionResponsable>();
    public ICollection<HistorialTrazabilidad> Historial { get; set; } = new List<HistorialTrazabilidad>();
    public ICollection<ComentarioSolicitud> Comentarios { get; set; } = new List<ComentarioSolicitud>();
}

[Table("evidencias_solicitud")]
public class EvidenciaSolicitud
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("solicitud_id")]
    public int SolicitudId { get; set; }

    [JsonIgnore]
    public Solicitud? Solicitud { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("ruta_archivo")]
    public string RutaArchivo { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("nombre_original")]
    public string NombreOriginal { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("tipo_mimetype")]
    public string TipoMimetype { get; set; } = string.Empty;

    [Column("tamano_bytes")]
    public long? TamanoBytes { get; set; }

    [Column("fecha_subida")]
    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;
}

[Table("asignaciones_responsables")]
public class AsignacionResponsable
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("solicitud_id")]
    public int SolicitudId { get; set; }

    [JsonIgnore]
    public Solicitud? Solicitud { get; set; }

    [Column("responsable_id")]
    public int ResponsableId { get; set; }
    public Usuario? Responsable { get; set; }

    [Column("notas")]
    public string? Notas { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("fecha_asignacion")]
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
}

[Table("historial_trazabilidad")]
public class HistorialTrazabilidad
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("solicitud_id")]
    public int SolicitudId { get; set; }

    [JsonIgnore]
    public Solicitud? Solicitud { get; set; }

    [Column("usuario_id")]
    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    [Column("accion")]
    public TipoAccionTrazabilidad Accion { get; set; }

    [Column("estado_anterior")]
    public EstadoSolicitud? EstadoAnterior { get; set; }

    [Column("estado_nuevo")]
    public EstadoSolicitud? EstadoNuevo { get; set; }

    [Required]
    [Column("detalle")]
    public string Detalle { get; set; } = string.Empty;

    [Column("fecha_registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}

[Table("comentarios_solicitud")]
public class ComentarioSolicitud
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("solicitud_id")]
    public int SolicitudId { get; set; }

    [JsonIgnore]
    public Solicitud? Solicitud { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    [Required]
    [Column("mensaje")]
    public string Mensaje { get; set; } = string.Empty;

    [Column("es_interno")]
    public bool EsInterno { get; set; } = false;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

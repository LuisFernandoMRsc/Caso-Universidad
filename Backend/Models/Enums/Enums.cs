namespace CampusConnect.Api.Models.Enums;

public enum RolUsuario
{
    ESTUDIANTE,
    ADMINISTRATIVO,
    TECNICO,
    ADMIN
}

public enum PrioridadSolicitud
{
    BAJA,
    MEDIA,
    ALTA,
    URGENTE
}

public enum EstadoSolicitud
{
    REGISTRADA,
    EN_EVALUACION,
    ASIGNADA,
    EN_PROCESO,
    RESUELTA,
    RECHAZADA,
    CERRADA
}

public enum CanalOrigen
{
    APP_MOVIL,
    PORTAL_WEB
}

public enum TipoRecurso
{
    AULA,
    LABORATORIO,
    EQUIPO,
    MOBILIARIO,
    OTRO
}

public enum TipoAccionTrazabilidad
{
    CREACION,
    CAMBIO_ESTADO,
    ASIGNACION,
    CAMBIO_PRIORIDAD,
    RESOLUCION,
    CIERRE,
    COMENTARIO,
    NOTA
}

using CampusConnect.Api.Models.Enums;

namespace CampusConnect.Api.DTOs;

public class DashboardMetricsDto
{
    public int TotalSolicitudes { get; set; }
    public int Pendientes { get; set; }
    public int Resueltas { get; set; }
    public int Cerradas { get; set; }
    public int Rechazadas { get; set; }
    public double TiempoPromedioResolucionHoras { get; set; }
    public double TiempoPromedioAsignacionHoras { get; set; }
    public double TasaEfectividad { get; set; }
}

public class CategoriaReporteDto
{
    public int CategoriaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public string? Color { get; set; }
    public int Cantidad { get; set; }
    public int Pendientes { get; set; }
    public int Resueltas { get; set; }
    public double Porcentaje { get; set; }
}

public class PrioridadReporteDto
{
    public PrioridadSolicitud Prioridad { get; set; }
    public int Cantidad { get; set; }
    public double Porcentaje { get; set; }
}

public class ResponsableReporteDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public string? Telefono { get; set; }
    public int TotalAsignadas { get; set; }
    public int Pendientes { get; set; }
    public int Resueltas { get; set; }
    public double TiempoPromedioHoras { get; set; }
}

public class CreateRecursoDto
{
    public string CodigoInventario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public TipoRecurso Tipo { get; set; } = TipoRecurso.EQUIPO;
    public string Ubicacion { get; set; } = string.Empty;
    public string Estado { get; set; } = "OPERATIVO";
}

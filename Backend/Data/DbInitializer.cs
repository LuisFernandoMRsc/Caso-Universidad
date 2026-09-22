using CampusConnect.Api.Models.Entities;
using CampusConnect.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampusConnect.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Asegurar que las tablas existan
        await context.Database.EnsureCreatedAsync();

        if (await context.Usuarios.AnyAsync())
        {
            return; // Ya está sembrada
        }

        // 1. Usuarios Iniciales
        var passAdmin = BCrypt.Net.BCrypt.HashPassword("Admin123!");
        var passStaff = BCrypt.Net.BCrypt.HashPassword("Soporte123!");
        var passTecnico = BCrypt.Net.BCrypt.HashPassword("Tecnico123!");
        var passEstudiante = BCrypt.Net.BCrypt.HashPassword("Estudiante123!");

        var admin = new Usuario
        {
            CarnetCodigo = "ADM-001",
            NombreCompleto = "Ing. Carlos Mendoza (Administrador General)",
            Email = "admin@campusconnect.edu",
            PasswordHash = passAdmin,
            Rol = RolUsuario.ADMIN,
            Telefono = "70011223"
        };

        var staff = new Usuario
        {
            CarnetCodigo = "ADM-002",
            NombreCompleto = "Lic. Sofia Ramirez (Coordinación de Servicios)",
            Email = "soporte@campusconnect.edu",
            PasswordHash = passStaff,
            Rol = RolUsuario.ADMINISTRATIVO,
            Telefono = "70022334"
        };

        var tecnico = new Usuario
        {
            CarnetCodigo = "TEC-001",
            NombreCompleto = "Juan Pérez (Técnico de Infraestructura y Redes)",
            Email = "tecnico@campusconnect.edu",
            PasswordHash = passTecnico,
            Rol = RolUsuario.TECNICO,
            Telefono = "70033445"
        };

        var estudiante1 = new Usuario
        {
            CarnetCodigo = "EST-2024-001",
            NombreCompleto = "Luis Fernando Martinez",
            Email = "estudiante@campusconnect.edu",
            PasswordHash = passEstudiante,
            Rol = RolUsuario.ESTUDIANTE,
            Telefono = "76543210"
        };

        var estudiante2 = new Usuario
        {
            CarnetCodigo = "EST-2024-002",
            NombreCompleto = "María Gómez Silva",
            Email = "maria.gomez@campusconnect.edu",
            PasswordHash = passEstudiante,
            Rol = RolUsuario.ESTUDIANTE,
            Telefono = "71234567"
        };

        await context.Usuarios.AddRangeAsync(admin, staff, tecnico, estudiante1, estudiante2);
        await context.SaveChangesAsync();

        // 2. Categorías Solicitudes
        var catMantenimiento = new CategoriaSolicitud
        {
            Nombre = "Mantenimiento",
            Descripcion = "Problemas de fontanería, electricidad, cerraduras, pintura y reparaciones físicas.",
            Icono = "build",
            Color = "#FF9800"
        };

        var catSoporte = new CategoriaSolicitud
        {
            Nombre = "Soporte Tecnológico",
            Descripcion = "Fallas de Wi-Fi universitario, proyectores, laboratorios de cómputo y plataformas.",
            Icono = "computer",
            Color = "#2196F3"
        };

        var catInfraestructura = new CategoriaSolicitud
        {
            Nombre = "Infraestructura",
            Descripcion = "Averías en aulas, ventanas, puertas, techos, graderías y áreas comunes.",
            Icono = "apartment",
            Color = "#4CAF50"
        };

        var catEquipamiento = new CategoriaSolicitud
        {
            Nombre = "Equipamiento",
            Descripcion = "Aires acondicionados, mobiliario de laboratorio, pupitres y equipos audiovisuales.",
            Icono = "devices",
            Color = "#9C27B0"
        };

        var catServicios = new CategoriaSolicitud
        {
            Nombre = "Servicios Institucionales",
            Descripcion = "Aseo, limpieza, cafetería, biblioteca y transporte universitario.",
            Icono = "local_convenience_store",
            Color = "#E91E63"
        };

        await context.Categorias.AddRangeAsync(catMantenimiento, catSoporte, catInfraestructura, catEquipamiento, catServicios);
        await context.SaveChangesAsync();

        // 3. Recursos Institucionales
        var rec1 = new RecursoInstitucional
        {
            CodigoInventario = "LAB-101-PC05",
            Nombre = "Computadora Desktop Laboratorio Redes",
            Tipo = TipoRecurso.LABORATORIO,
            Ubicacion = "Pabellón A, Piso 1, Laboratorio 101",
            Estado = "OPERATIVO"
        };

        var rec2 = new RecursoInstitucional
        {
            CodigoInventario = "AULA-204-PROY",
            Nombre = "Proyector Multimedia Epson HD",
            Tipo = TipoRecurso.EQUIPO,
            Ubicacion = "Edificio Central, Aula 204",
            Estado = "OPERATIVO"
        };

        var rec3 = new RecursoInstitucional
        {
            CodigoInventario = "AULA-301-AC",
            Nombre = "Aire Acondicionado 18000 BTU",
            Tipo = TipoRecurso.EQUIPO,
            Ubicacion = "Pabellón B, Aula 301",
            Estado = "DANADO"
        };

        var rec4 = new RecursoInstitucional
        {
            CodigoInventario = "BIB-WIFI-AP02",
            Nombre = "Access Point Wi-Fi Cisco Biblioteca",
            Tipo = TipoRecurso.EQUIPO,
            Ubicacion = "Biblioteca Central, 2do Piso",
            Estado = "OPERATIVO"
        };

        await context.Recursos.AddRangeAsync(rec1, rec2, rec3, rec4);
        await context.SaveChangesAsync();

        // 4. Solicitud 1 de demostración
        var sol1 = new Solicitud
        {
            TicketCode = "SOL-2026-0001",
            Titulo = "Proyector no enciende en Aula 204",
            Descripcion = "El proyector no da señal de encendido al conectar la laptop del docente, presenta luz roja parpadeante.",
            UbicacionDetallada = "Edificio Central, Aula 204, Proyector de techo",
            Prioridad = PrioridadSolicitud.ALTA,
            Estado = EstadoSolicitud.ASIGNADA,
            CanalOrigen = CanalOrigen.APP_MOVIL,
            EstudianteId = estudiante1.Id,
            CategoriaId = catSoporte.Id,
            RecursoId = rec2.Id,
            FechaCreacion = DateTime.UtcNow.AddDays(-1),
            FechaAsignacion = DateTime.UtcNow.AddHours(-12)
        };

        await context.Solicitudes.AddAsync(sol1);
        await context.SaveChangesAsync();

        var hist1 = new HistorialTrazabilidad
        {
            SolicitudId = sol1.Id,
            UsuarioId = estudiante1.Id,
            Accion = TipoAccionTrazabilidad.CREACION,
            EstadoAnterior = null,
            EstadoNuevo = EstadoSolicitud.REGISTRADA,
            Detalle = "Solicitud registrada mediante la App Móvil Campus Connect.",
            FechaRegistro = DateTime.UtcNow.AddDays(-1)
        };

        var hist2 = new HistorialTrazabilidad
        {
            SolicitudId = sol1.Id,
            UsuarioId = staff.Id,
            Accion = TipoAccionTrazabilidad.ASIGNACION,
            EstadoAnterior = EstadoSolicitud.REGISTRADA,
            EstadoNuevo = EstadoSolicitud.ASIGNADA,
            Detalle = $"Solicitud evaluada y asignada al técnico {tecnico.NombreCompleto}.",
            FechaRegistro = DateTime.UtcNow.AddHours(-12)
        };

        var asignacion1 = new AsignacionResponsable
        {
            SolicitudId = sol1.Id,
            ResponsableId = tecnico.Id,
            Notas = "Verificar fuente de poder y lámpara de proyector.",
            Activo = true,
            FechaAsignacion = DateTime.UtcNow.AddHours(-12)
        };

        var comentario1 = new ComentarioSolicitud
        {
            SolicitudId = sol1.Id,
            UsuarioId = tecnico.Id,
            Mensaje = "Estimado estudiante, nos acercaremos hoy a las 15:00 para revisar el equipo.",
            EsInterno = false,
            FechaCreacion = DateTime.UtcNow.AddHours(-10)
        };

        // Solicitud 2 (Resuelta)
        var sol2 = new Solicitud
        {
            TicketCode = "SOL-2026-0002",
            Titulo = "Gotera de agua en baño de varones Piso 2",
            Descripcion = "Existe una fuga constante en el lavamanos principal del segundo piso que genera charcos.",
            UbicacionDetallada = "Pabellón A, 2do Piso, Baño Varones",
            Prioridad = PrioridadSolicitud.URGENTE,
            Estado = EstadoSolicitud.RESUELTA,
            CanalOrigen = CanalOrigen.APP_MOVIL,
            EstudianteId = estudiante2.Id,
            CategoriaId = catMantenimiento.Id,
            FechaCreacion = DateTime.UtcNow.AddDays(-2),
            FechaAsignacion = DateTime.UtcNow.AddHours(-40),
            FechaResolucion = DateTime.UtcNow.AddHours(-8)
        };

        await context.Solicitudes.AddAsync(sol2);
        await context.SaveChangesAsync();

        var hist3 = new HistorialTrazabilidad
        {
            SolicitudId = sol2.Id,
            UsuarioId = estudiante2.Id,
            Accion = TipoAccionTrazabilidad.CREACION,
            EstadoAnterior = null,
            EstadoNuevo = EstadoSolicitud.REGISTRADA,
            Detalle = "Reporte registrado desde la aplicación móvil.",
            FechaRegistro = DateTime.UtcNow.AddDays(-2)
        };

        var hist4 = new HistorialTrazabilidad
        {
            SolicitudId = sol2.Id,
            UsuarioId = staff.Id,
            Accion = TipoAccionTrazabilidad.RESOLUCION,
            EstadoAnterior = EstadoSolicitud.EN_PROCESO,
            EstadoNuevo = EstadoSolicitud.RESUELTA,
            Detalle = "Reparación de válvula completada.",
            FechaRegistro = DateTime.UtcNow.AddHours(-8)
        };

        await context.HistorialTrazabilidad.AddRangeAsync(hist1, hist2, hist3, hist4);
        await context.Asignaciones.AddAsync(asignacion1);
        await context.Comentarios.AddAsync(comentario1);

        await context.SaveChangesAsync();
    }
}

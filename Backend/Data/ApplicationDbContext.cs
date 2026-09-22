using Microsoft.EntityFrameworkCore;
using CampusConnect.Api.Models.Entities;

namespace CampusConnect.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<CategoriaSolicitud> Categorias => Set<CategoriaSolicitud>();
    public DbSet<RecursoInstitucional> Recursos => Set<RecursoInstitucional>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<EvidenciaSolicitud> Evidencias => Set<EvidenciaSolicitud>();
    public DbSet<AsignacionResponsable> Asignaciones => Set<AsignacionResponsable>();
    public DbSet<HistorialTrazabilidad> HistorialTrazabilidad => Set<HistorialTrazabilidad>();
    public DbSet<ComentarioSolicitud> Comentarios => Set<ComentarioSolicitud>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique constraints
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.CarnetCodigo)
            .IsUnique();

        modelBuilder.Entity<CategoriaSolicitud>()
            .HasIndex(c => c.Nombre)
            .IsUnique();

        modelBuilder.Entity<RecursoInstitucional>()
            .HasIndex(r => r.CodigoInventario)
            .IsUnique();

        modelBuilder.Entity<Solicitud>()
            .HasIndex(s => s.TicketCode)
            .IsUnique();

        // Enums as strings or integers: store as strings for high readability in PostgreSQL
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Rol)
            .HasConversion<string>();

        modelBuilder.Entity<RecursoInstitucional>()
            .Property(r => r.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<Solicitud>()
            .Property(s => s.Prioridad)
            .HasConversion<string>();

        modelBuilder.Entity<Solicitud>()
            .Property(s => s.Estado)
            .HasConversion<string>();

        modelBuilder.Entity<Solicitud>()
            .Property(s => s.CanalOrigen)
            .HasConversion<string>();

        modelBuilder.Entity<HistorialTrazabilidad>()
            .Property(h => h.Accion)
            .HasConversion<string>();

        modelBuilder.Entity<HistorialTrazabilidad>()
            .Property(h => h.EstadoAnterior)
            .HasConversion<string>();

        modelBuilder.Entity<HistorialTrazabilidad>()
            .Property(h => h.EstadoNuevo)
            .HasConversion<string>();

        // Relationships
        modelBuilder.Entity<Solicitud>()
            .HasOne(s => s.Estudiante)
            .WithMany(u => u.SolicitudesCreadas)
            .HasForeignKey(s => s.EstudianteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Solicitud>()
            .HasOne(s => s.Categoria)
            .WithMany(c => c.Solicitudes)
            .HasForeignKey(s => s.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Solicitud>()
            .HasOne(s => s.Recurso)
            .WithMany(r => r.Solicitudes)
            .HasForeignKey(s => s.RecursoId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EvidenciaSolicitud>()
            .HasOne(e => e.Solicitud)
            .WithMany(s => s.Evidencias)
            .HasForeignKey(e => e.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AsignacionResponsable>()
            .HasOne(a => a.Solicitud)
            .WithMany(s => s.Asignaciones)
            .HasForeignKey(a => a.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AsignacionResponsable>()
            .HasOne(a => a.Responsable)
            .WithMany(u => u.Asignaciones)
            .HasForeignKey(a => a.ResponsableId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HistorialTrazabilidad>()
            .HasOne(h => h.Solicitud)
            .WithMany(s => s.Historial)
            .HasForeignKey(h => h.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HistorialTrazabilidad>()
            .HasOne(h => h.Usuario)
            .WithMany(u => u.HistorialTrazabilidad)
            .HasForeignKey(h => h.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ComentarioSolicitud>()
            .HasOne(c => c.Solicitud)
            .WithMany(s => s.Comentarios)
            .HasForeignKey(c => c.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ComentarioSolicitud>()
            .HasOne(c => c.Usuario)
            .WithMany(u => u.Comentarios)
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

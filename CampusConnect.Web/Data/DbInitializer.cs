using System;
using System.Collections.Generic;
using System.Linq;
using CampusConnect.Web.Models.Entities;

namespace CampusConnect.Web.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return; // DB ya tiene datos
            }

            // 1. Usuarios
            var admin = new User { Name = "Admin General", Email = "admin@campus.edu", PasswordHash = "admin123", Role = "Administrador" };
            var tecnico1 = new User { Name = "Carlos Técnico", Email = "soporte@campus.edu", PasswordHash = "soporte123", Role = "Soporte" };
            var estudiante1 = new User { Name = "Ana Estudiante", Email = "estudiante@campus.edu", PasswordHash = "estudiante123", Role = "Estudiante" };

            context.Users.AddRange(admin, tecnico1, estudiante1);
            context.SaveChanges();

            // 2. Recursos Universitarios
            var r1 = new Resource { Name = "Laboratorio de Cómputo 101", Type = "Laboratorio", Description = "30 Computadoras Core i7 con Windows 11", Status = "Activo" };
            var r2 = new Resource { Name = "Proyector Epson Aula 204", Type = "Equipamiento", Description = "Proyector HDMI de alta definición", Status = "Mantenimiento" };
            var r3 = new Resource { Name = "Aula Magna", Type = "Infraestructura", Description = "Auditorio principal con capacidad para 200 personas", Status = "Activo" };
            var r4 = new Resource { Name = "Router WiFi Piso 3", Type = "Soporte Tecnológico", Description = "Access Point Cisco para el bloque de ingeniería", Status = "Activo" };

            context.Resources.AddRange(r1, r2, r3, r4);
            context.SaveChanges();

            // 3. Solicitudes (Tickets)
            var t1 = new Ticket
            {
                Title = "Proyector no enciende en Aula 204",
                Description = "Al encender el switch, la luz roja parpadea y no emite imagen.",
                Status = "En Proceso",
                Priority = "Alta",
                Type = "Equipamiento",
                StudentId = estudiante1.Id,
                AssignedSupportId = tecnico1.Id,
                ResourceId = r2.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            var t2 = new Ticket
            {
                Title = "Falla de conexión WiFi en Piso 3",
                Description = "La señal se cae continuamente durante las clases virtuales.",
                Status = "Pendiente",
                Priority = "Media",
                Type = "Soporte Tecnológico",
                StudentId = estudiante1.Id,
                ResourceId = r4.Id,
                CreatedAt = DateTime.UtcNow.AddHours(-5)
            };

            var t3 = new Ticket
            {
                Title = "Mantenimiento preventivo PC Lab 101",
                Description = "Actualización de software de compiladores C# y NodeJS.",
                Status = "Resuelta",
                Priority = "Baja",
                Type = "Mantenimiento",
                StudentId = estudiante1.Id,
                AssignedSupportId = tecnico1.Id,
                ResourceId = r1.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            context.Tickets.AddRange(t1, t2, t3);
            context.SaveChanges();

            // 4. Comentarios de Trazabilidad
            var c1 = new TicketComment
            {
                TicketId = t1.Id,
                UserId = tecnico1.Id,
                CommentText = "Se revisó el cableado y se detectó falla en la lámpara. Se ordenó repuesto.",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            context.TicketComments.Add(c1);
            context.SaveChanges();
        }
    }
}

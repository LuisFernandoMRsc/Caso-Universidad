using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CampusConnect.Web.Data;
using CampusConnect.Web.Models.Entities;
using Xunit;

namespace CampusConnect.Tests
{
    public class CampusConnectTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        // Prueba 1: Inicio de sesión (Verificación de credenciales)
        [Fact]
        public async Task Test1_UserAuthentication_ValidCredentials_ReturnsUser()
        {
            // Arrange
            using var context = GetInMemoryDbContext("TestAuthDb");
            var user = new User
            {
                Name = "Administrador Test",
                Email = "admin@campus.edu",
                PasswordHash = "admin123",
                Role = "Administrador"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var authenticatedUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "admin@campus.edu" && u.PasswordHash == "admin123");

            // Assert
            Assert.NotNull(authenticatedUser);
            Assert.Equal("Administrador", authenticatedUser.Role);
        }

        // Prueba 2: Crear solicitud
        [Fact]
        public async Task Test2_CreateTicket_PersistsSuccessfully()
        {
            // Arrange
            using var context = GetInMemoryDbContext("TestTicketDb");
            var student = new User { Name = "Estudiante 1", Email = "estudiante@campus.edu", PasswordHash = "123", Role = "Estudiante" };
            var resource = new Resource { Name = "Laboratorio 101", Type = "Laboratorio", Status = "Activo" };
            context.Users.Add(student);
            context.Resources.Add(resource);
            await context.SaveChangesAsync();

            // Act
            var ticket = new Ticket
            {
                Title = "Falla de red en laboratorio",
                Description = "No hay acceso a internet",
                Status = "Pendiente",
                Priority = "Alta",
                Type = "Soporte Tecnológico",
                StudentId = student.Id,
                ResourceId = resource.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            // Assert
            var savedTicket = await context.Tickets.FirstOrDefaultAsync(t => t.Title == "Falla de red en laboratorio");
            Assert.NotNull(savedTicket);
            Assert.Equal("Pendiente", savedTicket.Status);
            Assert.Equal(student.Id, savedTicket.StudentId);
        }

        // Prueba 3: Adjuntar evidencia
        [Fact]
        public async Task Test3_AttachEvidence_LinksToTicketSuccessfully()
        {
            // Arrange
            using var context = GetInMemoryDbContext("TestEvidenceDb");
            var student = new User { Name = "Estudiante", Email = "est@campus.edu", PasswordHash = "123", Role = "Estudiante" };
            context.Users.Add(student);
            await context.SaveChangesAsync();

            var ticket = new Ticket
            {
                Title = "Lámpara rota",
                Description = "El foco del proyector explotó",
                Status = "Pendiente",
                Priority = "Media",
                Type = "Equipamiento",
                StudentId = student.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            // Act
            var evidence = new TicketEvidence
            {
                TicketId = ticket.Id,
                FilePath = "uploads/evidencias/lampara_rota.jpg",
                UploadedAt = DateTime.UtcNow
            };
            context.TicketEvidences.Add(evidence);
            await context.SaveChangesAsync();

            // Assert
            var ticketWithEvidence = await context.Tickets
                .Include(t => t.Evidences)
                .FirstOrDefaultAsync(t => t.Id == ticket.Id);

            Assert.NotNull(ticketWithEvidence);
            Assert.Single(ticketWithEvidence.Evidences);
            Assert.Equal("uploads/evidencias/lampara_rota.jpg", ticketWithEvidence.Evidences.First().FilePath);
        }

        // Prueba 4: Consultar seguimiento y cambio de estado
        [Fact]
        public async Task Test4_UpdateTicketStatus_TracksProgressionCorrectly()
        {
            // Arrange
            using var context = GetInMemoryDbContext("TestTrackingDb");
            var student = new User { Name = "Estudiante", Email = "est@campus.edu", PasswordHash = "123", Role = "Estudiante" };
            var tech = new User { Name = "Técnico Soporte", Email = "tech@campus.edu", PasswordHash = "123", Role = "Soporte" };
            context.Users.AddRange(student, tech);
            await context.SaveChangesAsync();

            var ticket = new Ticket
            {
                Title = "Pantalla no da video",
                Description = "Se necesita revisión de cables",
                Status = "Pendiente",
                Priority = "Baja",
                Type = "Equipamiento",
                StudentId = student.Id
            };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            // Act: Asignar técnico y cambiar a En Proceso
            ticket.AssignedSupportId = tech.Id;
            ticket.Status = "En Proceso";
            ticket.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            // Assert
            var updatedTicket = await context.Tickets.FindAsync(ticket.Id);
            Assert.NotNull(updatedTicket);
            Assert.Equal("En Proceso", updatedTicket.Status);
            Assert.Equal(tech.Id, updatedTicket.AssignedSupportId);
        }

        // Prueba 5: Visualizar comentarios de trazabilidad
        [Fact]
        public async Task Test5_ViewComments_RetrievesDiscussionHistory()
        {
            // Arrange
            using var context = GetInMemoryDbContext("TestCommentsDb");
            var student = new User { Name = "Estudiante", Email = "est@campus.edu", PasswordHash = "123", Role = "Estudiante" };
            var tech = new User { Name = "Técnico", Email = "tech@campus.edu", PasswordHash = "123", Role = "Soporte" };
            context.Users.AddRange(student, tech);
            await context.SaveChangesAsync();

            var ticket = new Ticket
            {
                Title = "Problema con audio",
                Description = "Micrófono con interferencia",
                Status = "Pendiente",
                Priority = "Media",
                Type = "Equipamiento",
                StudentId = student.Id
            };
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            // Act: Agregar comentarios
            var comment1 = new TicketComment
            {
                TicketId = ticket.Id,
                UserId = student.Id,
                CommentText = "Probé desconectar el cable y sigue haciendo ruido.",
                CreatedAt = DateTime.UtcNow
            };
            var comment2 = new TicketComment
            {
                TicketId = ticket.Id,
                UserId = tech.Id,
                CommentText = "Voy en camino al aula con un micrófono de repuesto.",
                CreatedAt = DateTime.UtcNow.AddMinutes(5)
            };
            context.TicketComments.AddRange(comment1, comment2);
            await context.SaveChangesAsync();

            // Assert
            var ticketComments = await context.TicketComments
                .Where(c => c.TicketId == ticket.Id)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            Assert.Equal(2, ticketComments.Count);
            Assert.Equal("Probé desconectar el cable y sigue haciendo ruido.", ticketComments[0].CommentText);
            Assert.Equal("Voy en camino al aula con un micrófono de repuesto.", ticketComments[1].CommentText);
        }
    }
}

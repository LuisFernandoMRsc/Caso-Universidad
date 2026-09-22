using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CampusConnect.Web.Models.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Role { get; set; } = "Estudiante"; // Administrador, Soporte, Estudiante

        // Relaciones
        public ICollection<Ticket> SubmittedTickets { get; set; } = new List<Ticket>();
        public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
    }
}

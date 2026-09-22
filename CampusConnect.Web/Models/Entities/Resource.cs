using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CampusConnect.Web.Models.Entities
{
    public class Resource
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Type { get; set; } = string.Empty; // Aula, Proyector, Laboratorio, etc.

        public string? Description { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Activo"; // Activo, Mantenimiento, Inactivo

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusConnect.Web.Models.Entities
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Pendiente"; // Pendiente, En Proceso, Resuelta, Cerrada

        [Required, MaxLength(50)]
        public string Priority { get; set; } = "Media"; // Baja, Media, Alta, Urgente

        [Required, MaxLength(100)]
        public string Type { get; set; } = "Mantenimiento"; // Mantenimiento, Soporte Tecnológico, Infraestructura, etc.

        // Foreign Keys
        public int StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public User Student { get; set; } = null!;

        public int? AssignedSupportId { get; set; }
        [ForeignKey(nameof(AssignedSupportId))]
        public User? AssignedSupport { get; set; }

        public int? ResourceId { get; set; }
        [ForeignKey(nameof(ResourceId))]
        public Resource? Resource { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
        public ICollection<TicketEvidence> Evidences { get; set; } = new List<TicketEvidence>();
    }
}

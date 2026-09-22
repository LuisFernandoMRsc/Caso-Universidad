using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusConnect.Web.Models.Entities
{
    public class TicketEvidence
    {
        [Key]
        public int Id { get; set; }

        public int TicketId { get; set; }
        [ForeignKey(nameof(TicketId))]
        public Ticket Ticket { get; set; } = null!;

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = string.Empty; // Ruta del archivo/imagen

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using quickBook.Models;

namespace quickBook.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [ForeignKey("Organizer")]
        public int OrganizerId { get; set; }
        
        public User Organizer { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? MeetingLink { get; set; }

        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public int StatusId { get; set; }

        [ForeignKey("StatusId")]
        public AppointmentStatus Status { get; set; }

        public string? CancelReason { get; set; }
        
        public bool IsCancelled { get; set; }
    }
}
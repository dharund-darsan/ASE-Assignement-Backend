using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace quickBook.Dtos
{
    public class AppointmentConflictCheckDto
    {
        [Required]
        public int OrganizerId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public List<int> ParticipantIds { get; set; } = new();
    }
}
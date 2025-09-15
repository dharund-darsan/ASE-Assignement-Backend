using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace quickBook.Models
{
    public class AppointmentRecurrence
    {
        [Key]
        public int RecurrenceId { get; set; }

        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        [Required, MaxLength(20)]
        public string Frequency { get; set; } = "Weekly"; // Daily, Weekly, Monthly, Yearly

        public int Interval { get; set; } = 1; // e.g., every 1 week, every 2 months

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Occurrences { get; set; }

        // Custom fields
        public string? DaysOfWeek { get; set; } // JSON ["monday","thursday"]
        public int? DayOfMonth { get; set; } // e.g., 15
        public int? MonthOfYear { get; set; } // e.g., 12 for December
    }
}
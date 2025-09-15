// Dtos/AppointmentCreateDto.cs
namespace quickBook.Dtos
{
    public class AppointmentCreateDto
    {
        public int OrganizerId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? MeetingLink { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int StatusId { get; set; }
        public List<int> ParticipantIds { get; set; } = new();

        // Recurrence
        public string? Frequency { get; set; } // daily, weekly, monthly, yearly
        public int? Interval { get; set; }
        public DateTime? RecurrenceStartDate { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
        public List<string>? DaysOfWeek { get; set; } // ["monday","thursday"]
        public int? DayOfMonth { get; set; }
        public int? MonthOfYear { get; set; }
    }
}
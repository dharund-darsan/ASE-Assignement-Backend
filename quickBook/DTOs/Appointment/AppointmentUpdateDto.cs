namespace quickBook.Dtos
{
    public class AppointmentUpdateDto
    {
        public int AppointmentId { get; set; }
        public int OrganizerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? MeetingLink { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int StatusId { get; set; }
        public List<int> ParticipantIds { get; set; } = new();

        // 🔹 Recurrence fields
        public string? Frequency { get; set; } 
        public int? Interval { get; set; }      
        public DateTime? RecurrenceStartDate { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
        public List<string>? DaysOfWeek { get; set; }
        public int? DayOfMonth { get; set; }    
        public int? MonthOfYear { get; set; }   
    }
}
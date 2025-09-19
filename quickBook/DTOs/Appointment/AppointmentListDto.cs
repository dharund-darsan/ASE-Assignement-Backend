namespace quickBook.DTOs
{
    public class AppointmentListDto
    {
        public int AppointmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? MeetingLink { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = "#000000";
        public int OrganizerId { get; set; } 
        public List<int> Participants { get; set; } = new();
		public string? Frequency { get; set; }
    public int? Interval { get; set; }
    public DateTime? RecurrenceStartDate { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }
    public List<string>? DaysOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public int? MonthOfYear { get; set; }
    }
}
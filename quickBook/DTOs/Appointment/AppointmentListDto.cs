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
        public string OrganizerName { get; set; } = string.Empty;
        public List<string> Participants { get; set; } = new();
    }
}
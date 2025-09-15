using quickBook.Models;

namespace quickBook.Models
{
    public class AppointmentParticipant
    {
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
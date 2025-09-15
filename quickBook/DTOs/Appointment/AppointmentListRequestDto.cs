using System;

namespace quickBook.Dtos
{
    public class AppointmentListRequestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
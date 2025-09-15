using System.ComponentModel.DataAnnotations;

namespace quickBook.Models
{
    public class AppointmentStatus
    {
        [Key]
        public int StatusId { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        [Required, MaxLength(20)]
        public string ColorCode { get; set; } = "#000000";
    }
}
using quickBook.Dtos;
using quickBook.Models;

namespace quickBook.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<Appointment> CreateAppointmentAsync(AppointmentCreateDto dto);
        Task<Appointment> UpdateAppointmentAsync(AppointmentUpdateDto dto);
        Task CancelAppointmentAsync(AppointmentCancelDto dto);
        Task<List<Appointment>> GetAppointmentsAsync(int userId, DateTime startDate, DateTime endDate);
        Task<List<Appointment>> CheckConflictsAsync(AppointmentConflictCheckDto dto);
    }
}
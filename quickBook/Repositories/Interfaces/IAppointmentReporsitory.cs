using quickBook.Models;

namespace quickBook.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<Appointment?> GetByIdAsync(int appointmentId);
        Task<List<Appointment>> GetAppointmentsForUserAsync(int userId, DateTime startDate, DateTime endDate);
        Task AddAsync(Appointment appointment);
        Task UpdateAsync(Appointment appointment);
        Task DeleteAsync(Appointment appointment);
        Task SaveChangesAsync();
        Task<List<Appointment>> CheckConflictsAsync(int organizerId, DateTime startTime, DateTime endTime, List<int> participantIds, int? excludeAppointmentId = null);
    }
}
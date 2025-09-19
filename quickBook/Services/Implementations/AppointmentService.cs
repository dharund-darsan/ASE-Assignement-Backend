using quickBook.Dtos;
using quickBook.Mappers;
using quickBook.Models;
using quickBook.Repositories.Interfaces;
using quickBook.Services.Interfaces;

namespace quickBook.Services.Implementation
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public AppointmentService(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<Appointment> CreateAppointmentAsync(AppointmentCreateDto dto)
        {
            var appointment = AppointmentMapper.ToEntity(dto);
            await _appointmentRepository.AddAsync(appointment);
            await _appointmentRepository.SaveChangesAsync();
            return appointment;
        }

        public async Task<Appointment> UpdateAppointmentAsync(AppointmentUpdateDto dto)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(dto.AppointmentId);
            if (appointment == null)
                throw new Exception("Appointment not found.");

            // Apply updates
            appointment.Title = dto.Title;
            appointment.Description = dto.Description;
            appointment.StartTime = dto.StartTime;
            appointment.EndTime = dto.EndTime;

            await _appointmentRepository.UpdateAsync(appointment);
            await _appointmentRepository.SaveChangesAsync();

            return appointment;
        }

        public async Task CancelAppointmentAsync(AppointmentCancelDto dto)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(dto.AppointmentId);
            if (appointment == null)
                throw new Exception("Appointment not found.");

            appointment.IsCancelled = true;
            appointment.CancelReason = dto.CancelReason;

            await _appointmentRepository.UpdateAsync(appointment);
            await _appointmentRepository.SaveChangesAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _appointmentRepository.GetAppointmentsForUserAsync(userId, startDate, endDate);
        }

        public async Task<List<Appointment>> CheckConflictsAsync(AppointmentConflictCheckDto dto)
        {
            return await _appointmentRepository.CheckConflictsAsync(dto.OrganizerId, dto.StartTime, dto.EndTime, dto.ParticipantIds);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using quickBook.Data;
using quickBook.Models;
using quickBook.Repositories.Interfaces;
using System.Text.Json;

namespace quickBook.Repositories.Implementation
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment?> GetByIdAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Organizer)
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        }

        public async Task<List<Appointment>> GetAppointmentsForUserAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Appointments
                .Include(a => a.Organizer)
                .Include(a => a.Status)
                .Where(a => !a.IsCancelled &&
                            (a.OrganizerId == userId ||
                             _context.AppointmentParticipants.Any(p => p.AppointmentId == a.AppointmentId && p.UserId == userId)) &&
                            a.EndTime >= startDate && a.StartTime <= endDate)
                .ToListAsync();
        }

        public async Task AddAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
        }

        public async Task UpdateAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
        }

        public async Task DeleteAsync(Appointment appointment)
        {
            _context.Appointments.Remove(appointment);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<Appointment>> CheckConflictsAsync(
            int organizerId,
            DateTime startTime,
            DateTime endTime,
            List<int> participantIds,
            int? excludeAppointmentId = null)
        {
            // reuse your existing CheckConflictsAsync implementation
            var usersInvolved = new List<int> { organizerId };
            usersInvolved.AddRange(participantIds ?? new List<int>());

            var baseAppointments = await _context.Appointments
                .Where(a => !a.IsCancelled &&
                            (excludeAppointmentId == null || a.AppointmentId != excludeAppointmentId) &&
                            (
                                usersInvolved.Contains(a.OrganizerId) ||
                                _context.AppointmentParticipants.Any(ap => ap.AppointmentId == a.AppointmentId &&
                                                                           usersInvolved.Contains(ap.UserId))
                            ))
                .Include(a => a.Organizer)
                .ToListAsync();

            return baseAppointments; // you can include recurrence expansion logic if needed
        }
    }
}

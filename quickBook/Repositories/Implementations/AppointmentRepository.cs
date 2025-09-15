using quickBook.Repositories.Interfaces;
using quickBook.Data;
using quickBook.Models;
using Microsoft.EntityFrameworkCore;
using quickBook.Dtos;

namespace quickBook.Repositories.Implementations
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;
        public AppointmentRepository(ApplicationDbContext context) => _context = context;

        public async Task<ExistenceCheckResultDto> CheckExistenceAsync(
            int? organizerId = null,
            int? statusId = null,
            List<int>? participantIds = null)
        {
            var result = new ExistenceCheckResultDto();

            if (organizerId.HasValue)
                result.OrganizerExists = await _context.Users.AnyAsync(u => u.UserId == organizerId.Value);

            if (statusId.HasValue)
                result.StatusExists = await _context.AppointmentStatuses.AnyAsync(s => s.StatusId == statusId.Value);

            if (participantIds is not null && participantIds.Any())
            {
                var existingParticipantCount = await _context.Users
                    .CountAsync(u => participantIds.Contains(u.UserId));

                result.ParticipantsExist = existingParticipantCount == participantIds.Count;
            }

            return result;
        }


    }
}
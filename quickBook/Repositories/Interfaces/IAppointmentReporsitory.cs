using quickBook.Dtos;
using quickBook.Models;

namespace quickBook.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<ExistenceCheckResultDto> CheckExistenceAsync(int? organizerId, int? statusId, List<int>? participantIds);
    }
}
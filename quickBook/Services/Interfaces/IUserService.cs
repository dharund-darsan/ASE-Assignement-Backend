using quickBook.DTOs;

namespace quickBook.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserListDto?> RegisterAsync(RegisterUserDto dto);
        Task<UserListDto?> LoginAsync(LoginDto dto);
        Task<List<UserListDto>> GetUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
    }
}
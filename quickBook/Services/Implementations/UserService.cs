using quickBook.DTOs;
using quickBook.Mappers;
using quickBook.Repositories.Interfaces;
using quickBook.Services.Interfaces;

namespace quickBook.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository) 
        {
            _userRepository = userRepository;
        }

        public async Task<UserListDto?> RegisterAsync(RegisterUserDto dto)
        {
            var existing = await _userRepository.GetByEmailAsync(dto.Email);
            if (existing != null) return null;

            var user = UserMapper.ToUser(dto);
            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();

            return UserMapper.ToUserDto(user);
        }

        public async Task<UserListDto?> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return UserMapper.ToUserDto(user);
        }

        public async Task<List<UserListDto>> GetUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(UserMapper.ToUserDto).ToList();
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : UserMapper.ToDetailedUserDto(user);
        }
    }
}
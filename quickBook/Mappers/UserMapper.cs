using quickBook.DTOs;
using quickBook.Models;

namespace quickBook.Mappers
{
    public static class UserMapper
    {
        public static UserListDto ToUserDto(User user) => new UserListDto
        {
            UserId = user.UserId,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
        };

        public static UserDto ToDetailedUserDto(User user) => new UserDto
        {
            UserId = user.UserId,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            Role = user.Role,
            DateOfBirth = user.DateOfBirth,
            UpdatedAt = user.UpdatedAt,
            CreatedAt = user.CreatedAt
        };

        public static User ToUser(RegisterUserDto dto) => new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            DateOfBirth = dto.DateOfBirth,
            Role = "User"
        };
    }
}
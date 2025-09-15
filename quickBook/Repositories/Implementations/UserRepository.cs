using Microsoft.EntityFrameworkCore;
using quickBook.Data;
using quickBook.Models;
using quickBook.Repositories.Interfaces;

namespace quickBook.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context) => _context = context;

        public async Task<User?> GetByIdAsync(int id) =>
            await _context.Users.FindAsync(id);

        public async Task<User?> GetByEmailAsync(string email) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<List<User>> GetAllAsync() =>
            await _context.Users.ToListAsync();

        public async Task AddAsync(User user) =>
            await _context.Users.AddAsync(user);

        public async Task SaveAsync() =>
            await _context.SaveChangesAsync();
    }
}
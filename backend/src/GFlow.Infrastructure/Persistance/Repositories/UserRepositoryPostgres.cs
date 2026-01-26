using GFlow.Application.Ports;
using GFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GFlow.Infrastructure.Persistance.Repositories
{
    public class UserRepositoryPostgres : IUserRepository
    {

        public async Task<User?> Add(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return await Get(user.Id);
        }
        
        private readonly AppDbContext _context;

        public UserRepositoryPostgres(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> Get(string id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> Update(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetByRefreshToken(string refreshToken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<IEnumerable<User>> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<User>();
            
            // Case-insensitive search on Username or Email
            return await _context.Users
                .Where(u => EF.Functions.Like(u.Username, $"%{term}%") || EF.Functions.Like(u.Email, $"%{term}%"))
                .Take(10) // Limit results
                .ToListAsync();
        }
    }
}
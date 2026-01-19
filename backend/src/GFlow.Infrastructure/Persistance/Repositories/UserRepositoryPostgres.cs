using GFlow.Application.Ports;
using GFlow.Domain.Entities;
using GFlow.Infrastructure.Persistance.Migrations;

namespace GFlow.Infrastructure.Persistance.Repositories
{
    public class UserRepositoryPostgres : IUserRepository
    {

        public User? Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }
        
        private readonly AppDbContext _context;

        public UserRepositoryPostgres(AppDbContext context)
        {
            _context = context;
        }

        public User? Get(string id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }

        public User? GetByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }
    }
}
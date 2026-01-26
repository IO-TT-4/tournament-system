using GFlow.Domain.Entities;

namespace GFlow.Application.Ports
{
    public interface IUserRepository
    {

        public Task<User?> Get(string id);
        public Task<User?> GetByUsername(string username);
        public Task<User?> Add(User user);
        public Task<User?> Update(User user);
        public Task<User?> GetByRefreshToken(string refreshToken);
        public Task<IEnumerable<User>> GetAll();
        public Task<IEnumerable<User>> Search(string term);
    }
}
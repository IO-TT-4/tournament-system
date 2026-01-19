using GFlow.Domain.Entities;

namespace GFlow.Application.Ports
{
    public interface IUserRepository
    {

        public User? Get(string id);
        public User? GetByUsername(string username);
        public User? Add(User user);
    }
}
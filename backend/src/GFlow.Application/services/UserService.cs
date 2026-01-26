using GFlow.Application.DTOs;
using GFlow.Domain.Entities;
using GFlow.Application.Ports;
using System.Threading.Tasks;

namespace GFlow.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<User?> GetUser(string id) => await _userRepo.Get(id);
        public async Task<IEnumerable<User>> GetAllUsers() => await _userRepo.GetAll();
        public async Task<IEnumerable<User>> SearchUsers(string term) => await _userRepo.Search(term);

    }
}
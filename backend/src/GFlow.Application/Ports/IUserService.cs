using GFlow.Application.DTOs;
using GFlow.Domain.Entities;

namespace GFlow.Application.Ports
{
    public interface IUserService
    {
        public User? Login(LoginRequest request);
        public User? RegisterUser(RegisterRequest request);
        public User? GetUser(string id);
    }
}
using GFlow.Application.DTOs;
using GFlow.Domain.Entities;

namespace GFlow.Application.Ports
{
    public interface IUserService
    {
        public Task<User> GetUser(string id);
    }
}
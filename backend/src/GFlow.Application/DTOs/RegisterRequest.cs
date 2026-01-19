namespace GFlow.Application.DTOs
{
    public class RegisterRequest
    {
        public required string username { get; set; }
        public required string password { get; set; }
        public required string email { get; set; }
    }
}
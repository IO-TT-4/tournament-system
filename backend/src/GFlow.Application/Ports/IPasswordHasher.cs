namespace GFlow.Application.Ports
{
    public interface IPasswordHasher
    {
        public string HashPassword(string password);

        public bool VerifyPassword(string password, string storedHash);
    }
}
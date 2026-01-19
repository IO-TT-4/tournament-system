using System.Security.Cryptography;
using GFlow.Application.Ports;

namespace GFlow.Infrastructure.Security
{
    public sealed class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int KeySize = 32;

        public string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256
            );

            byte[] key = pbkdf2.GetBytes(KeySize);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }

        public bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.', 3);
            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] storedKey = Convert.FromBase64String(parts[2]);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256
            );

            byte[] computedKey = pbkdf2.GetBytes(storedKey.Length);

            return CryptographicOperations.FixedTimeEquals(storedKey, computedKey);
        }
    }
}
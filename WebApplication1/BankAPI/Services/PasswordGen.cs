using System.Security.Cryptography;

namespace WebApplication1.BankAPI.Services
{
    public static class PasswordGen
    {
        private const int Iterations = 10000;

        public static void HashPassword(string password, out string hash, out string salt)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                Iterations,
                HashAlgorithmName.SHA256,
                32);

            salt = Convert.ToBase64String(saltBytes);
            hash = Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPassword(string password, string hash, string salt)
        {
            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                password,
                Convert.FromBase64String(salt),
                Iterations,
                HashAlgorithmName.SHA256,
                32);

            isValid = CryptographicOperations.FixedTimeEquals(hashBytes, Convert.FromBase64String(hash));

            return isValid;
        }
    }
}

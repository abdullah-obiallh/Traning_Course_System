using System.Security.Cryptography;
using System.Text;

namespace TrainingCoursesSystem.Server.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();

            var passwordBytes = Encoding.UTF8.GetBytes(password);

            var hashBytes = sha256.ComputeHash(passwordBytes);

            var hash = new StringBuilder();

            foreach (var b in hashBytes)
            {
                hash.Append(b.ToString("x2"));
            }

            return hash.ToString();
        }

        public static bool VerifyPassword(string password, string savedHash)
        {
            var passwordHash = HashPassword(password);

            return passwordHash == savedHash;
        }
    }
}
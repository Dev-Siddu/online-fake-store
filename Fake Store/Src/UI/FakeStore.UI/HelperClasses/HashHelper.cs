using System.Security.Cryptography;
using System.Text;

namespace FakeStoreUI.HelperClasses
{
    public static class HashHelper
    {
        public static string HashPassword(string password)
        {
            string hashedPassword = string.Empty;
            byte[] bytePass = Encoding.UTF8.GetBytes(password);
            using SHA256 sha256 = SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(bytePass,0,bytePass.Length);

            hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

            return hashedPassword;
        }

    }
}

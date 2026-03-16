using System.Security.Cryptography;
using System.Text;

namespace CyberRegistration;

public class CryptoService
{
    public string Hasher(string password)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = SHA256.HashData(passwordBytes);
        return Convert.ToHexString(hashBytes);
    }
}
using System.Security.Cryptography;
using System.Text;

namespace SICAPI.Models.Helpers;

public class EncryptGetSHA1
{
    public static string GetSHA1(string str)
    {
        SHA1 sha1 = SHA1Managed.Create();
        ASCIIEncoding encoding = new ASCIIEncoding();
        byte[] stream = null;
        StringBuilder sb = new StringBuilder();
        stream = sha1.ComputeHash(encoding.GetBytes(str));
        for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);

        return sb.ToString();
    }

    public static string CryptoProvider(string _pass)
    {
        SHA1CryptoServiceProvider elProveedor = new SHA1CryptoServiceProvider();
        byte[] vectoBytes = System.Text.Encoding.UTF8.GetBytes(_pass);
        byte[] inArray = elProveedor.ComputeHash(vectoBytes);
        elProveedor.Clear();
        _pass = Convert.ToBase64String(inArray);

        return _pass;
    }
}
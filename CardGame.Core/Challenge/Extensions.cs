using System.Text;

namespace CardGame.Core.Challenge
{
    public static class Extensions
    {
        public static byte[] Encrypt(this ICryptoService cryptoService, int value, byte[] key)
        {
            return cryptoService.Encrypt(Encoding.UTF8.GetBytes(value.ToString()), key);
        }

        public static int DecryptInt(this ICryptoService cryptoService, byte[] encrypted, byte[] key)
        {
            return int.Parse(Encoding.UTF8.GetString(cryptoService.Decrypt(encrypted, key)));
        }
    }
}
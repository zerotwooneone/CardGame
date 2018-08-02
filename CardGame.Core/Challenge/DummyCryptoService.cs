namespace CardGame.Core.Challenge
{
    public class DummyCryptoService : ICryptoService
    {
        public byte[] Encrypt(byte[] value, byte[] key)
        {
            return value;
        }

        public byte[] Decrypt(byte[] encrypted, byte[] key)
        {
            return encrypted;
        }
    }
}
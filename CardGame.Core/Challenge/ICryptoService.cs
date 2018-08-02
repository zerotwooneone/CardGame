namespace CardGame.Core.Challenge
{
    public interface ICryptoService
    {
        byte[] Encrypt(byte[] value, byte[] key);
        byte[] Decrypt(byte[] encrypted, byte[] key);
    }
}
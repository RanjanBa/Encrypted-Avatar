public class EncryptedMessage
{
    public string encSessionKey;
    public string tag;
    public string ciphertext;
    public string nonce;

    public override string ToString()
    {
        return ciphertext;
    }
}

using System.Collections.Generic;

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

    // public bool ParseEncryptedMessage(List<string> _sentences, out EncryptedMessage _encryptedMsg) {
    //     string encSessionKey = "";
    //     string tag = "";
    //     string ciphertext = "";
    //     string nonce = "";

    //     for (int i = 0; i < _sentences.Count; i++)
    //     {
    //         List<string> words = ParseSentence(_sentences[i]);


    //         if (words[0].Equals("enc_session_key"))
    //         {
    //             encSessionKey = words[1];
    //         }
    //         else if (words[0].Equals("tag"))
    //         {
    //             tag = words[1];
    //         }
    //         else if (words[0].Equals("ciphertext"))
    //         {
    //             ciphertext = words[1];
    //         }
    //         else if (words[0].Equals("nonce"))
    //         {
    //             nonce = words[1];
    //         }
    //     }

    //     _encryptedMsg = new EncryptedMessage
    //     {
    //         encSessionKey = encSessionKey,
    //         tag = tag,
    //         ciphertext = ciphertext,
    //         nonce = nonce,
    //     };

    //     return true;
    // }
}

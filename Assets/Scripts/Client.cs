using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using UnityEngine.Events;

public class Client : MonoBehaviour
{
    private const int m_DATA_BUFFER_SIZE = 10240;

    public string alias = "Ranjan";

    [SerializeField]
    private string ipAddress = "127.0.0.1";
    [SerializeField]
    private int port = 9000;

    private TcpClient m_client;
    private NetworkStream m_stream;
    private Thread m_listeningDataThread;

    private string m_privateKey;
    private string m_publicKey;
    private EncryptedMessage m_encryptedMessage;

    private bool m_isDecryptedMsgReceived;
    private string m_decryptedMsg;

    public UnityAction<string> onDecryptedMsgReceived;

    private void Start()
    {
        ConnectToServer();
    }

    private void Update()
    {
        if(m_isDecryptedMsgReceived)
        {
            onDecryptedMsgReceived(m_decryptedMsg);
            m_isDecryptedMsgReceived = false;
            m_decryptedMsg = "";
        }
    }

    private void ConnectToServer()
    {
        try
        {
            m_client = new TcpClient(ipAddress, port);
            m_stream = m_client.GetStream();
            Debug.Log("Connected to server.");

            m_listeningDataThread = new Thread(new ThreadStart(ListeningForMsgFromServer))
            {
                IsBackground = true
            };
            m_listeningDataThread.Start();

            SendMessageToServer(alias + "\n");
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e.ToString());
        }
    }

    public void SendMessageToServer(string _message)
    {
        if (m_client == null || !m_client.Connected)
        {
            Debug.LogError("Client not connected to server.");
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(_message);
        m_stream.Write(data, 0, data.Length);
        Debug.Log(alias + " sent message to server : " + _message);
    }

    private void ListeningForMsgFromServer()
    {
        try
        {
            byte[] bytes = new byte[m_DATA_BUFFER_SIZE];
            while (true)
            {
                // Check if there's any data available on the network stream
                if (m_stream.DataAvailable)
                {
                    int length;
                    // Read incoming stream into byte array.
                    while ((length = m_stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incomingData = new byte[length];
                        Array.Copy(bytes, 0, incomingData, 0, length);
                        // Convert byte array to string message.
                        string serverMessage = Encoding.UTF8.GetString(incomingData);
                        Debug.Log(alias + " received msg from server : " + serverMessage);
                        List<string> sentences = ParseMessage(serverMessage);

                        if (sentences.Count == 0)
                        {
                            return;
                        }

                        if (sentences[0] == "generate_key")
                        {
                            ParseKeys(sentences);
                        }
                        else if (sentences[0] == "encrypt_msg")
                        {
                            ParseEncryptedMessage(sentences);
                        }
                        else if (sentences[0] == "decrypt_msg")
                        {
                            ParseDecryptedMessage(sentences);
                        }
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private void OnApplicationQuit()
    {
        m_client?.Close();
        m_listeningDataThread?.Abort();
    }

    public void EncryptMsg(string _msg)
    {
        if (m_publicKey == null || m_publicKey.Length == 0)
        {
            Debug.Log("Valid public key is not present.");
            return;
        }
        if (m_client != null && m_client.Connected)
        {
            Debug.Log("Getting Encrypted Msg...");
            string new_msg = string.Format("encrypt_msg\nmsg:{0}\npublic_key:{1}", _msg, m_publicKey);
            Debug.Log(new_msg);
            SendMessageToServer(new_msg);
        }
    }

    public void DecryptMsg()
    {
        if (m_encryptedMessage == null)
        {
            Debug.Log("No Encrypted Msg To Decrypt...");
            return;
        }

        if (m_client != null && m_client.Connected)
        {
            Debug.Log("Getting Decrypted Msg...");

            string new_msg = string.Format("decrypt_msg\nprivate_key:{0}\nenc_session_key:{1}\ntag:{2}\nciphertext:{3}\nnonce:{4}", m_privateKey, m_encryptedMessage.encSessionKey, m_encryptedMessage.tag, m_encryptedMessage.ciphertext, m_encryptedMessage.nonce);
            Debug.Log(new_msg);
            SendMessageToServer(new_msg);
        }
    }

    private void ParseKeys(List<string> _sentences)
    {
        Debug.Log("Parsing Public & Private Key...");

        for (int i = 0; i < _sentences.Count; i++)
        {
            List<string> words = ParseSentence(_sentences[i]);

            if (words[0].Equals("public_key"))
            {
                m_publicKey = words[1];
            }
            else if (words[0].Equals("private_key"))
            {
                m_privateKey = words[1];
            }

        }

        Debug.Log("Parsing Public & Private Key Completed...");
    }

    private void ParseEncryptedMessage(List<string> _sentences)
    {
        Debug.Log("Parsing Encrypted Msg...");
        string encSessionKey = "";
        string tag = "";
        string ciphertext = "";
        string nonce = "";

        for (int i = 0; i < _sentences.Count; i++)
        {
            List<string> words = ParseSentence(_sentences[i]);


            if (words[0].Equals("enc_session_key"))
            {
                encSessionKey = words[1];
            }
            else if (words[0].Equals("tag"))
            {
                tag = words[1];
            }
            else if (words[0].Equals("ciphertext"))
            {
                ciphertext = words[1];
            }
            else if (words[0].Equals("nonce"))
            {
                nonce = words[1];
            }
        }

        m_encryptedMessage = new EncryptedMessage
        {
            encSessionKey = encSessionKey,
            tag = tag,
            ciphertext = ciphertext,
            nonce = nonce,
        };

        Debug.Log("Parsing Encrypted Msg Completed...");
        DecryptMsg();
    }

    private void ParseDecryptedMessage(List<string> _sentences)
    {
        Debug.Log("Parsing Decrypted Msg...");
        string msg = "";
        for(int i = 1; i < _sentences.Count; i++) {
            msg += _sentences[i];
            msg += "\n";
        }
        m_decryptedMsg = msg;
        m_isDecryptedMsgReceived = true;
        Debug.Log("Parsing Encrypted Msg Completed...");
    }

    private List<string> ParseMessage(string _msg)
    {
        string sentence = "";
        List<string> sentences = new List<string>();
        for (int i = 0; i < _msg.Length; i++)
        {
            if (_msg[i] == '\n')
            {
                sentence = RemoveLeadingTrailing(sentence);
                if (sentence != "")
                {
                    sentences.Add(sentence);
                }
                sentence = "";
            }
            else
            {
                sentence += _msg[i];
            }
        }

        sentence = RemoveLeadingTrailing(sentence);
        if (sentence != "")
        {
            sentences.Add(sentence);
        }

        return sentences;
    }

    private List<string> ParseSentence(string _text)
    {
        Debug.Log(_text);
        List<string> words = new List<string>();
        string word = "";
        for (int i = 0; i < _text.Length; i++)
        {
            if (_text[i] == ':')
            {
                word = RemoveLeadingTrailing(word);
                if (word != "")
                {
                    words.Add(word);
                }
                word = "";
            }
            else
            {
                word += _text[i];
            }
        }

        word = RemoveLeadingTrailing(word);
        if (word != "")
        {
            words.Add(word);
        }

        return words;
    }

    private string RemoveLeadingTrailing(string _text, char _ch = ' ')
    {
        int start_idx = 0;
        while (start_idx < _text.Length && _text[start_idx] == _ch)
        {
            start_idx++;
        }

        int end_idx = _text.Length - 1;
        while (end_idx >= 0 && _text[end_idx] == _ch)
        {
            end_idx--;
        }

        return _text.Substring(start_idx, end_idx - start_idx + 1);
    }
}

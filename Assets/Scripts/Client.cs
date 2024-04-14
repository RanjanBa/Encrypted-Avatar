using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Client : MonoBehaviour
{
    private const int m_DATA_BUFFER_SIZE = 10240;

    public string ip = "127.0.0.1";
    public int port = 9000;
    public string alias = "Ranjan";

    private TcpClient m_socket;
    private NetworkStream m_stream;
    private byte[] m_receiveBuffer;
    private string m_privateKey;
    private string m_publicKey;
    private EncryptedMessage m_encryptedMessage;

    private void Start()
    {
        m_socket = new TcpClient()
        {
            ReceiveBufferSize = m_DATA_BUFFER_SIZE,
            SendBufferSize = m_DATA_BUFFER_SIZE
        };

        m_receiveBuffer = new byte[m_DATA_BUFFER_SIZE];
        IAsyncResult _result = m_socket.BeginConnect(ip, port, ConnectCallback, m_socket);
    }

    private void ConnectCallback(IAsyncResult _result)
    {
        m_socket.EndConnect(_result);

        if (!m_socket.Connected)
        {
            Debug.Log("Can't connect to the server!");
            return;
        }

        m_stream = m_socket.GetStream();
        m_stream.BeginRead(m_receiveBuffer, 0, m_DATA_BUFFER_SIZE, InitializeReceiveCallback, null);
    }

    private void InitializeReceiveCallback(IAsyncResult _result)
    {
        int _byteLength = m_stream.EndRead(_result);
        if (_byteLength <= 0)
        {
            Debug.Log("No message send ...");
            return;
        }

        string msg = ReceiveMsgDecoding(_byteLength);
        try
        {
            byte[] _data = Encoding.UTF8.GetBytes(alias);
            m_stream.BeginWrite(_data, 0, _data.Length, null, null);
            m_stream.BeginRead(m_receiveBuffer, 0, m_DATA_BUFFER_SIZE, (_result) => {
                int _byteLength = m_stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    Debug.Log("No message send ...");
                    return;
                }
                ReceiveMsgDecoding(_byteLength);
            }, null);
        }
        catch
        {
            Debug.Log("Error!");
        }
    }

    private void OnApplicationQuit()
    {
        if(m_socket != null && m_socket.Connected) {
            // Debug.Log("Application ending after " + Time.time + " seconds");
            m_socket.Close();
        }
    }

    [ContextMenu("Get Key")]
    public void GetKey()
    {
        if (m_socket != null && m_socket.Connected)
        {
            Debug.Log("Getting key...");
            string msg = "get_key";
            byte[] _data = Encoding.UTF8.GetBytes(msg);
            m_stream.BeginWrite(_data, 0, _data.Length, null, null);
            m_stream.BeginRead(m_receiveBuffer, 0, m_DATA_BUFFER_SIZE, KeyReceiveMsgCallback, null);
        }
        else
        {
            Debug.Log("Not connected to the server!");
        }
    }

    public void EncryptMsg(string _msg)
    {
        if(m_publicKey == null || m_publicKey.Length == 0)
        {
            Debug.Log("Valid public key is not present.");
            return;
        }
        if(m_socket != null && m_socket.Connected)
        {
            Debug.Log("Getting Encrypted Msg...");
            string new_msg = string.Format("encrypt_msg\nmsg:{0}\npublic_key:{1}", _msg, m_publicKey);
            Debug.Log(new_msg);
            byte[] _data = Encoding.UTF8.GetBytes(new_msg);
            m_stream.BeginWrite(_data, 0, _data.Length, null, null);
            m_stream.BeginRead(m_receiveBuffer, 0, m_DATA_BUFFER_SIZE, EncryptedReceiveMsgCallback, null);
        }
    }

    public void DecryptMsg()
    {
        if(m_encryptedMessage == null)
        {
            Debug.Log("No Encrypted Msg To Decrypt...");
            return;
        }

        if (m_socket != null && m_socket.Connected)
        {
            Debug.Log("Getting Decrypted Msg...");

            string new_msg = string.Format("decrypt_msg\nprivate_key:{0}\nenc_session_key:{1}\ntag:{2}\nciphertext:{3}\nnonce:{4}", m_privateKey, m_encryptedMessage.encSessionKey, m_encryptedMessage.tag, m_encryptedMessage.ciphertext, m_encryptedMessage.nonce);
            Debug.Log(new_msg);
            byte[] _data = Encoding.UTF8.GetBytes(new_msg);
            m_stream.BeginWrite(_data, 0, _data.Length, null, null);
            m_stream.BeginRead(m_receiveBuffer, 0, m_DATA_BUFFER_SIZE, DecryptedReceiveMsgCallback, null);
        }
    }

    private void KeyReceiveMsgCallback(IAsyncResult _result)
    {
        try
        {
            int _byteLength = m_stream.EndRead(_result);
            if (_byteLength <= 0)
            {
                Debug.Log("No message send ...");
                return;
            }

            string msg = ReceiveMsgDecoding(_byteLength);
            if (msg == "") return;

            Debug.Log("Parsing Public & Private Key...");

            List<string> sentences = ParseMessage(msg);

            for (int i = 0; i < sentences.Count; i++)
            {
                List<string> words = ParseSentence(sentences[i]);

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
        catch
        {
            Debug.Log("Error in Receiving Msg!");
        }
    }

    private void EncryptedReceiveMsgCallback(IAsyncResult _result)
    {
        try
        {
            int _byteLength = m_stream.EndRead(_result);
            if (_byteLength <= 0)
            {
                Debug.Log("No message send ...");
                return;
            }

            string msg = ReceiveMsgDecoding(_byteLength);
            if (msg == "") return;
            Debug.Log("Parsing Encrypted Msg...");

            List<string> sentences = ParseMessage(msg);
            string enc_session_key = "";
            string tag = "";
            string ciphertext = "";
            string nonce = "";
            for (int i = 0; i < sentences.Count; i++)
            {
                List<string> words = ParseSentence(sentences[i]);

                if (words[0].Equals("enc_session_key"))
                {
                    enc_session_key = words[1];
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

            m_encryptedMessage = new EncryptedMessage()
            {
                encSessionKey = enc_session_key,
                tag = tag,
                ciphertext = ciphertext,
                nonce = nonce,
            };

            Debug.Log(m_encryptedMessage);
            Debug.Log("Parsing Encrypted Msg Completed...");
        }
        catch
        {
            Debug.Log("Error in Receiving Msg!");
        }
    }

    private void DecryptedReceiveMsgCallback(IAsyncResult _result)
    {
        try
        {
            int _byteLength = m_stream.EndRead(_result);
            if (_byteLength <= 0)
            {
                Debug.Log("No message send ...");
                return;
            }

            string msg = ReceiveMsgDecoding(_byteLength);
        }
        catch
        {
            Debug.Log("Error in Receiving Msg!");
        }
    }

    private string ReceiveMsgDecoding(int _byteLength)
    {
        byte[] _data = new byte[_byteLength];
        Array.Copy(m_receiveBuffer, _data, _byteLength);

        string msg = Encoding.UTF8.GetString(_data);
        Debug.Log("byte length : " + _byteLength);
        Debug.Log(msg);
        return msg;
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
        for(int i = 0; i < _text.Length; i++)
        {
            if (_text[i] == ':')
            {
                word = RemoveLeadingTrailing(word);
                if(word != "")
                {
                    words.Add(word);
                }
                word = "";
            } else
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
        while(end_idx >= 0 && _text[end_idx] == _ch)
        {
            end_idx--;
        }

        return _text.Substring(start_idx, end_idx - start_idx + 1);
    }
}

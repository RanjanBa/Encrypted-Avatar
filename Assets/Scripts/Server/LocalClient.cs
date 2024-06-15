using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using Newtonsoft.Json;

public class LocalClient : MonoBehaviour
{
    private const int m_DATA_BUFFER_SIZE = 10240;

    [SerializeField]
    private string m_ipAddress = "127.0.0.1";
    [SerializeField]
    private int m_port = 9000;

    private TcpClient m_localClient;
    private NetworkStream m_stream;
    private Thread m_receiveThread;

    public Action<string, string> onKeyGenerated;
    public Action<Dictionary<string, string>> onEncryptedMsgReceived;
    public Action<string> onDecryptedMsgReceived;
    public Action onConnectedWithServer;

    private string m_gameObjectName;

    private void Start()
    {
        ConnectToServer();

#if UNITY_EDITOR
        onDecryptedMsgReceived += (_msg) =>
        {
            Debug.Log(_msg);
        };
#endif
        m_gameObjectName = gameObject.name;
    }

    private void OnApplicationQuit()
    {
        m_localClient?.Close();
        m_receiveThread?.Abort();
    }

    private void ConnectToServer()
    {
        try
        {
            m_localClient = new TcpClient(m_ipAddress, m_port);
            m_stream = m_localClient.GetStream();
#if UNITY_EDITOR
            Debug.Log("Connected to local server...");
#endif
            onConnectedWithServer?.Invoke();

            m_receiveThread = new Thread(new ThreadStart(ListeningServerForMsg))
            {
                IsBackground = true
            };
            m_receiveThread.Start();
        }
        catch (SocketException e)
        {
#if UNITY_EDITOR
            Debug.LogError("SocketException: " + e.ToString());
#endif
        }
    }

    private void ListeningServerForMsg()
    {
        try
        {
            byte[] _bytes = new byte[m_DATA_BUFFER_SIZE];
            while (true)
            {
                // Check if there's any data available on the network stream
                if (m_stream.DataAvailable)
                {
                    int _length;
                    // Read incoming stream into byte array.
                    while ((_length = m_stream.Read(_bytes, 0, _bytes.Length)) != 0)
                    {
                        var incomingData = new byte[_length];
                        Array.Copy(_bytes, 0, incomingData, 0, _length);
                        // Convert byte array to string message.
                        string _serverMsg = Encoding.UTF8.GetString(incomingData);
#if UNITY_EDITOR
                        if (_serverMsg.Length != 0)
                        {
                            Debug.Log(m_gameObjectName + " received msg from server : " + _serverMsg);
                        }
                        else
                        {
                            Debug.Log(m_gameObjectName + " received no msg from server.");
                            continue;
                        }
#endif
                        ParseMessage(_serverMsg);
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
#if UNITY_EDITOR
            Debug.Log("Socket exception: " + socketException.ToString());
#endif
        }
    }

    private void ParseMessage(string _message)
    {
        if (_message.Length == 0)
        {
            return;
        }
        try
        {
            Dictionary<string, string> _parsedMsg = JsonConvert.DeserializeObject<Dictionary<string, string>>(_message);
            if (_parsedMsg.TryGetValue(Keys.INSTRUCTION, out string _msgCode))
            {
                if (_msgCode == Instructions.GENERATE_KEY)
                {
                    ParseKeys(_parsedMsg);
                }
                else if (_msgCode == Instructions.ENCRYPT_MSG)
                {
                    ParseEncryptedMessage(_parsedMsg);
                }
                else if (_msgCode == Instructions.DECRYPT_MSG)
                {
                    ParseDecryptedMessage(_parsedMsg);
                }
#if UNITY_EDITOR
                else
                {
                    Debug.Log("Message is sent without proper instruction -> " + _msgCode);
                }
#endif
            }
#if UNITY_EDITOR
            else
            {
                Debug.Log("No instruction is given with the msg...");
                if (_parsedMsg.TryGetValue(Keys.MESSAGE, out string _msg))
                {
                    Debug.Log(_msg);
                }
            }
#endif
        }
        catch (JsonReaderException e)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Json deserializatio error : " + e.ToString());
#endif
        }
    }

    private void ParseKeys(Dictionary<string, string> _parsedMsg)
    {
#if UNITY_EDITOR
        Debug.Log("Parsing Public & Private Key...");
#endif

        if (!_parsedMsg.TryGetValue(Keys.PUBLIC_KEY, out string _publicKey))
        {
#if UNITY_EDITOR
            Debug.Log("No Public Key is given in the msg.");
#endif
            return;
        }

        if (!_parsedMsg.TryGetValue(Keys.PRIVATE_KEY, out string _privateKey))
        {
#if UNITY_EDITOR
            Debug.Log("No Private Key is given in the msg.");
#endif
            return;
        }

        onKeyGenerated?.Invoke(_publicKey, _privateKey);
#if UNITY_EDITOR
        Debug.Log("Parsing Public & Private Key Completed...");
#endif
    }

    private void ParseEncryptedMessage(Dictionary<string, string> _parsedMsg)
    {
#if UNITY_EDITOR
        Debug.Log("Parsing Encrypted Msg...");
#endif
        try
        {
            string encSessionKey = _parsedMsg[Keys.ENC_SESSION_KEY];
            string tag = _parsedMsg[Keys.TAG];
            string cipherText = _parsedMsg[Keys.CIPHER_TEXT];
            string nonce = _parsedMsg[Keys.NONCE];

            Dictionary<string, string> _encryptedMsg = new Dictionary<string, string>() {
                {Keys.ENC_SESSION_KEY, encSessionKey},
                {Keys.TAG, tag},
                {Keys.CIPHER_TEXT, cipherText},
                {Keys.NONCE, nonce}
            };
#if UNITY_EDITOR
            Debug.Log("Parsing Encrypted Msg Completed...");
#endif
            onEncryptedMsgReceived?.Invoke(_encryptedMsg);
        }
#if UNITY_EDITOR
        catch (ArgumentException e)
        {
            Debug.LogWarning("Error in parsing Encrypted Msg " + e.Message);
            Debug.Log("Parsing Encrypted Msg Failed...");
        }
#endif
    }

    private void ParseDecryptedMessage(Dictionary<string, string> _parsedMsg)
    {
#if UNITY_EDITOR
        Debug.Log("Parsing Decrypted Msg...");
#endif

        if (!_parsedMsg.TryGetValue(Keys.MESSAGE, out string _msg))
        {
#if UNITY_EDITOR
            Debug.Log("No Msg Key is given in the msg.");
#endif
            return;
        }

        onDecryptedMsgReceived?.Invoke(_msg);

#if UNITY_EDITOR
        Debug.Log("Parsing Decrypted Msg Completed...");
#endif
    }

    public void SendMessageToServer(string _message)
    {
        if (m_localClient == null || !m_localClient.Connected)
        {
#if UNITY_EDITOR
            Debug.LogError("Client not connected to server.");
#endif
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(_message);
        m_stream.Write(data, 0, data.Length);
#if UNITY_EDITOR
        Debug.Log(m_gameObjectName + " sent message to server : " + _message);
#endif
    }

    public void EncryptMsg(string _message, string _publicKey)
    {
        if (_publicKey == null || _publicKey.Length == 0)
        {
#if UNITY_EDITOR
            Debug.Log("Valid public key is not present.");
#endif
            return;
        }

        if (m_localClient == null || !m_localClient.Connected)
        {
#if UNITY_EDITOR
            Debug.Log("Local Client is connected to the local server.");
#endif
            return;
        }

#if UNITY_EDITOR
        Debug.Log("Encrypting Msg...");
#endif
        Dictionary<string, string> _msgDict = new Dictionary<string, string>
        {
            { Keys.INSTRUCTION, Instructions.ENCRYPT_MSG },
            { Keys.PUBLIC_KEY, _publicKey },
            { Keys.MESSAGE, _message }
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
#if UNITY_EDITOR
        Debug.Log(_msg);
#endif
        SendMessageToServer(_msg);
    }

    public void DecryptMsg(Dictionary<string, string> _encryptedMsg, string _privateKey)
    {
        if (m_localClient == null || !m_localClient.Connected)
        {
#if UNITY_EDITOR
            Debug.Log("Local Client is connected to the local server.");
#endif
            return;
        }

#if UNITY_EDITOR
        Debug.Log("Decrypting Msg...");
#endif
        Dictionary<string, string> _msgDict = new Dictionary<string, string> {
            {Keys.INSTRUCTION, Instructions.DECRYPT_MSG },
            {Keys.PRIVATE_KEY, _privateKey}
        };

        foreach (KeyValuePair<string, string> item in _encryptedMsg)
        {
            _msgDict.Add(item.Key, item.Value);
        }

        string _msg = JsonConvert.SerializeObject(_msgDict);
#if UNITY_EDITOR
        Debug.Log(_msg);
#endif
        SendMessageToServer(_msg);
    }
}

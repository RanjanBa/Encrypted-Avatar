using System.Text;
using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Client : MonoBehaviour
{
    private const int m_DATA_BUFFER_SIZE = 10240;

    [SerializeField]
    private string m_ipAddress = "127.0.0.1";
    [SerializeField]
    private int m_port = 12000;

    private TcpClient m_client;
    private NetworkStream m_stream;
    private Thread m_receiverThread;

    public Action onConnectedWithServer;

    private void Start()
    {
        ConnectToServer();
    }

    private void OnApplicationQuit()
    {
        m_client?.Close();
        m_client?.Dispose();
        m_receiverThread?.Abort();
    }

    private void ConnectToServer()
    {
        try
        {
            m_client = new TcpClient(m_ipAddress, m_port);
            m_stream = m_client.GetStream();
            Debug.Log("Connected to the main server...");
            onConnectedWithServer?.Invoke();

            m_receiverThread = new Thread(new ThreadStart(ListeningServerForMsg));
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException : " + e.ToString());
        }
    }

    private void ListeningServerForMsg()
    {
        try
        {
            byte[] _data = new byte[m_DATA_BUFFER_SIZE];

            if (m_stream.DataAvailable)
            {
                int _length;
                while ((_length = m_stream.Read(_data, 0, _data.Length)) != 0)
                {
                    byte[] _incomingData = new byte[_length];
                    Array.Copy(_data, 0, _incomingData, 0, _length);
                    string _serverMsg = Encoding.UTF8.GetString(_incomingData);

#if UNITY_EDITOR
                    if (_serverMsg.Length != 0)
                    {
                        Debug.Log(gameObject.name.ToUpper() + " received msg from server : " + _serverMsg);
                    }
                    else
                    {
                        Debug.Log(gameObject.name.ToUpper() + " received no msg from server.");
                        continue;
                    }
#endif
                    ParseMessage(_serverMsg);
                }
            }
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException : " + e.ToString());
        }
    }

    private void ParseMessage(string _message)
    {
        if (_message.Length == 0)
        {
            return;
        }

        Dictionary<string, string> _parsedMsg = JsonConvert.DeserializeObject<Dictionary<string, string>>(_message);

        
    }
}

using System.Text;
using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

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

    private string m_gameObjectName;

    public Action onConnectedWithServer;
    public Action<string> onMessageReceived;

    private void Start()
    {
        m_gameObjectName = gameObject.name;
        ConnectToServer();
    }

    private void OnApplicationQuit()
    {
        m_client?.Close();
        m_client?.Dispose();
        m_receiverThread?.Abort();
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
                        Debug.Log(m_gameObjectName + " received msg from server : " + _serverMsg);
                    }
                    else
                    {
                        Debug.Log(m_gameObjectName + " received no msg from server.");
                        continue;
                    }
#endif
                    onMessageReceived?.Invoke(_serverMsg);
                }
            }
        }

        catch (SocketException e)
        {
#if UNITY_EDITOR
            Debug.LogError("SocketException : " + e.ToString());
#endif
        }
    }

    public void ConnectToServer()
    {
        try
        {
            m_client = new TcpClient(m_ipAddress, m_port);
            m_stream = m_client.GetStream();
#if UNITY_EDITOR
            Debug.Log("Connected to the main server...");
#endif
            onConnectedWithServer?.Invoke();

            m_receiverThread = new Thread(new ThreadStart(ListeningServerForMsg))
            {
                IsBackground = true
            };
            m_receiverThread.Start();
        }
        catch (SocketException e)
        {
#if UNITY_EDITOR
            Debug.LogError("SocketException : " + e.ToString());
#endif
        }
    }

    public void SendMessageToServer(string _message)
    {
        if (m_client == null || !m_client.Connected)
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
}

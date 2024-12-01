using System.Text;
using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ServerClient
{
    private const int m_DATA_BUFFER_SIZE = 8192;

    private TcpClient m_client;
    private NetworkStream m_stream;
    private Thread m_receiverThread;

    public Action onConnectedWithServer;
    public Action<string> onMessageReceived;

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
                        Debug.Log(m_client.Client.ToString() + " received msg from main server : " + _serverMsg);
                    }
                    else
                    {
                        Debug.Log(m_client.Client.ToString() + " received no msg from main server.");
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
            Debug.LogError("Server SocketException : " + e.ToString());
#endif
        }
    }

    public void ConnectToServer(string _ipAddress, int _port)
    {
        try
        {
            m_client = new TcpClient(_ipAddress, _port);
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
            CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg(e.ToString().Truncate(130), 1f));
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

        byte[] _data = Encoding.UTF8.GetBytes(_message);
        m_stream.Write(_data, 0, _data.Length);
#if UNITY_EDITOR
        Debug.Log(m_client.Client.ToString() + " client is sending message to server : " + _message);
#endif
    }

    public void Disconnect()
    {
        m_client?.Close();
        m_client?.Dispose();
        m_receiverThread?.Abort();
    }
}

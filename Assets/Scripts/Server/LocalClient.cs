using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using Newtonsoft.Json;

public class LocalClient
{
    private const int m_DATA_BUFFER_SIZE = 40560;

    private TcpClient m_localClient;
    private NetworkStream m_stream;
    private Thread m_receiveThread;

    public Action onConnectedWithServer;
    public Action<string> onMessageReceived;

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
                            Debug.Log(m_localClient.Client.ToString() + " received msg from local server : " + _serverMsg);
                        }
                        else
                        {
                            Debug.Log(m_localClient.Client.ToString() + " received no msg from local server.");
                            continue;
                        }
#endif
                        onMessageReceived?.Invoke(_serverMsg);
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
#if UNITY_EDITOR
            Debug.Log("Local Socket exception: " + socketException.ToString());
#endif
        }
    }

    public void ConnectToServer(string _ipAddress, int _port)
    {
        try
        {
            m_localClient = new TcpClient(_ipAddress, _port);
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
            CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg(e.ToString().Truncate(130), 1f));
        }
    }

    public void SendMessageToServer(string _message)
    {
        if (m_localClient == null || !m_localClient.Connected)
        {
#if UNITY_EDITOR
            Debug.LogError("Local client is not connected to local server.");
#endif
            return;
        }

        byte[] _data = Encoding.UTF8.GetBytes(_message);
        m_stream.Write(_data, 0, _data.Length);
#if UNITY_EDITOR
        Debug.Log(m_localClient.Client.ToString() + " client sent message to local server : " + _message);
#endif
    }

    public void Disconnect()
    {
        m_localClient?.Close();
        m_localClient?.Dispose();
        m_receiveThread?.Abort();
    }
}

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

            m_receiverThread = new Thread(new ThreadStart(ListeningServer));
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException : " + e.ToString());
        }
    }

    private void ListeningServer()
    {
        try
        {
            byte[] _data = new byte[m_DATA_BUFFER_SIZE];

            if(m_stream.DataAvailable) {
                int _length;
                while((_length = m_stream.Read(_data, 0, _data.Length)) != 0) {
                    byte[] _incomingData = new byte[_length];
                    Array.Copy(_data, 0, _incomingData, 0, _length);
                    string _serverMsg = Encoding.UTF8.GetString(_incomingData);
                }
            }
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException : " + e.ToString());
        }
    }
}

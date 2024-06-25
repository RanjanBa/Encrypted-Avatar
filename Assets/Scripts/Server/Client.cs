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

    private string m_serverPublicKey;

    private string m_gameObjectName;

    public Action onConnectedWithServer;
    public Action<AvatarInfo> onAvatarCreated;
    public Action<WorldInfo> onWorldCreated;
    public Action<List<AvatarInfo>> onAllAvatarsRetrieved;
    public Action<List<WorldInfo>> onAllWorldsRetrieved;
    public Action<JoinInfo> onWorldJoined;
    public Action<Dictionary<string, string>> onMessageRecieved;

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
                    ParseMessage(_serverMsg);
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
                if (_msgCode == Instructions.SERVER_KEY)
                {
                    m_serverPublicKey = _parsedMsg[Keys.PUBLIC_KEY];
#if UNITY_EDITOR
                    Debug.Log("Server Public Key -> " + m_serverPublicKey.Truncate(50));
                    if (_parsedMsg.TryGetValue(Keys.MESSAGE, out string _msg))
                    {
                        Debug.Log(_msg);
                    }
#endif
                }
                else if (_msgCode == Instructions.CREATE_AVATAR)
                {
                    AvatarInfo _avatarInfo = new AvatarInfo()
                    {
                        avatarId = _parsedMsg[Keys.AVATAR_ID],
                        avatarName = _parsedMsg[Keys.AVATAR_NAME]
                    };
                    onAvatarCreated?.Invoke(_avatarInfo);
                }
                else if (_msgCode == Instructions.CREATE_WORLD)
                {
                    WorldInfo _worldInfo = new WorldInfo()
                    {
                        worldId = _parsedMsg[Keys.WORLD_ID],
                        worldName = _parsedMsg[Keys.WORLD_NAME]
                    };
                    onWorldCreated?.Invoke(_worldInfo);
                }
                else if (_msgCode == Instructions.CLIENT_ALL_AVATARS)
                {
                    List<AvatarInfo> _avatars = new List<AvatarInfo>();
                    int idx = 0;
                    while (true)
                    {
                        if (_parsedMsg.TryGetValue(idx.ToString(), out string _sentence))
                        {
                            string[] strs = _sentence.Split(',');
                            AvatarInfo _info = new AvatarInfo()
                            {
                                avatarId = strs[0],
                                avatarName = strs[1],
                                avatarViewId = strs[2]
                            };
                            _avatars.Add(_info);
                            idx++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    onAllAvatarsRetrieved?.Invoke(_avatars);
                }
                else if (_msgCode == Instructions.ALL_WORLDS)
                {
                    List<WorldInfo> _worlds = new List<WorldInfo>();
                    int idx = 0;
                    while (true)
                    {
                        if (_parsedMsg.TryGetValue(idx.ToString(), out string _sentence))
                        {
                            string[] strs = _sentence.Split(',');
                            WorldInfo _info = new WorldInfo()
                            {
                                worldId = strs[0],
                                worldName = strs[1],
                                worldViewId = strs[2]
                            };
                            _worlds.Add(_info);
                            idx++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    onAllWorldsRetrieved?.Invoke(_worlds);
                }
                else if (_msgCode == Instructions.JOIN_WORLD)
                {
                    JoinInfo _joinInfo = new JoinInfo()
                    {
                        worldInfo = new WorldInfo()
                        {
                            worldId = _parsedMsg[Keys.WORLD_ID],
                            worldName = _parsedMsg[Keys.WORLD_NAME],
                            worldViewId = _parsedMsg[Keys.WORLD_VIEW_ID]
                        },
                        avatarInfo = new AvatarInfo()
                        {
                            avatarId = _parsedMsg[Keys.AVATAR_ID],
                            avatarName = _parsedMsg[Keys.AVATAR_NAME],
                            avatarViewId = _parsedMsg[Keys.AVATAR_VIEW_ID]
                        }
                    };
                    onWorldJoined?.Invoke(_joinInfo);
                }
                else if (_msgCode == Instructions.WORLD_ALL_AVATARS)
                {
                    List<AvatarInfo> _avatars = new List<AvatarInfo>();
                    int idx = 0;
                    while (true)
                    {
                        if (_parsedMsg.TryGetValue(idx.ToString(), out string _sentence))
                        {
                            string[] strs = _sentence.Split(',');
                            AvatarInfo _info = new AvatarInfo()
                            {
                                avatarId = strs[0],
                                avatarName = strs[1],
                                avatarViewId = strs[2]
                            };
                            _avatars.Add(_info);
                            idx++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    onAllAvatarsRetrieved?.Invoke(_avatars);
                }
                else if (_msgCode == Instructions.SEND_MSG)
                {
                    Dictionary<string, string> _msgInfo = new Dictionary<string, string>() {
                        {Keys.WORLD_ID, _parsedMsg[Keys.WORLD_ID]},
                        {Keys.AVATAR_ID, _parsedMsg[Keys.AVATAR_ID]},
                        {Keys.MESSAGE, _parsedMsg[Keys.MESSAGE]}
                    };
                    onMessageRecieved?.Invoke(_msgInfo);
                }
                else if (_msgCode == Instructions.ERROR)
                {
#if UNITY_EDITOR
                    Debug.Log("Error occurs in server side...");
#endif
                    if (_parsedMsg.TryGetValue(Keys.MESSAGE, out string _msg))
                    {
#if UNITY_EDITOR
                        Debug.Log(_msg);
#endif
                        CanvasManager.Instance.errorMsg.Enqueue(new ErrorMsg(_msg, 1f));
                    }
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

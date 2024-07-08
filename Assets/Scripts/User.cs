using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[RequireComponent(typeof(Client))]
public class User : MonoBehaviour
{
    [SerializeField]
    private string m_serverIpAddress = "127.0.0.1";
    [SerializeField]
    private int m_serverPort = 12000;
    [SerializeField]
    private string m_localIpAddress = "127.0.0.1";
    [SerializeField]
    private int m_localPort = 9000;

    private string m_privateKey;
    private string m_publicKey;
    private string m_serverPublicKey;
    private string m_usedId;

    private Client m_mainServerClient;
    private LocalClient m_localServerClient;

    public string UserId => m_usedId;

    public ProcessHandler<AvatarInfo> logInProcess;
    public ProcessHandler<string> registrationProcess;
    public ProcessHandler<AvatarInfo> avatarCreationProcess;
    public ProcessHandler<WorldInfo> worldCreationProcess;
    public ProcessHandler<List<AvatarInfo>> getAllAvatarsProcess;
    public ProcessHandler<List<WorldInfo>> getAllWorldsProcess;
    public ProcessHandler<JoinInfo> worldJoinnedProcess;
    public ProcessHandler<Dictionary<string, string>> messageReceivedProcess;

    private void Awake()
    {
        m_mainServerClient = GetComponent<Client>();
        m_localServerClient = GetComponent<LocalClient>();

        logInProcess = new ProcessHandler<AvatarInfo>();
        avatarCreationProcess = new ProcessHandler<AvatarInfo>();
        worldCreationProcess = new ProcessHandler<WorldInfo>();
        getAllAvatarsProcess = new ProcessHandler<List<AvatarInfo>>();
        getAllWorldsProcess = new ProcessHandler<List<WorldInfo>>();
        worldJoinnedProcess = new ProcessHandler<JoinInfo>();
        messageReceivedProcess = new ProcessHandler<Dictionary<string, string>>();
    }

    private void Start()
    {
        worldJoinnedProcess.Subscribe((_joinInfo) =>
        {
            GameManager.Instance.UpdateSelectedWorld(_joinInfo.worldInfo);
        });

        m_mainServerClient.onMessageReceived += (_msg) =>
        {
            ParseMessage(_msg);
        };

        m_localServerClient.onKeyGenerated += (pk, sk) =>
        {
            m_publicKey = pk;
            m_privateKey = sk;
            SendPublicKeyToServer(m_publicKey);
        };

        m_localServerClient.onEncryptedMsgReceived += (_info) =>
        {
            _info[Keys.MSG_TYPE] = MessageType.ENCRYPTED_TEXT;
            string _msg = JsonConvert.SerializeObject(_info);
            m_mainServerClient.SendMessageToServer(_msg);
        };

        m_localServerClient.onDecryptedMsgReceived += (_info) =>
        {
            ParseMessage(_info);
        };

        GenerateKey();
    }

    private void Update()
    {
        logInProcess.UpdateProcess();
        avatarCreationProcess.UpdateProcess();
        worldCreationProcess.UpdateProcess();
        getAllAvatarsProcess.UpdateProcess();
        getAllWorldsProcess.UpdateProcess();
        worldJoinnedProcess.UpdateProcess();
        messageReceivedProcess.UpdateProcess();
        messageReceivedProcess.ChangeProcessToRunning();
    }

    private void OnEnable()
    {
        messageReceivedProcess.Subscribe(GameManager.Instance.OnMessageReceived);
    }

    private void OnDisable()
    {
        messageReceivedProcess.Unsubscribe(GameManager.Instance.OnMessageReceived);
    }

    private void OnAvatarCreated(AvatarInfo _avatarInfo)
    {
        avatarCreationProcess.ChangeProcessToCompleted(_avatarInfo);
    }

    private void OnWorldCreated(WorldInfo _worldInfo)
    {
        worldCreationProcess.ChangeProcessToCompleted(_worldInfo);
    }

    private void OnAllAvatarsRetrieved(List<AvatarInfo> _avatars)
    {
        getAllAvatarsProcess.ChangeProcessToCompleted(_avatars);
    }

    private void OnAllWorldsRetrieved(List<WorldInfo> _worlds)
    {
        getAllWorldsProcess.ChangeProcessToCompleted(_worlds);
    }

    private void OnWorldJoinned(JoinInfo _joinInfo)
    {
        worldJoinnedProcess.ChangeProcessToCompleted(_joinInfo);
    }

    private void OnMessageReceived(Dictionary<string, string> _msgInfo)
    {
        messageReceivedProcess.ChangeProcessToCompleted(_msgInfo);
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
            if (_parsedMsg.TryGetValue(Keys.MSG_TYPE, out string _msgType))
            {
                if (_msgType == MessageType.ENCRYPTED_TEXT)
                {
                    DecryptMsg(_parsedMsg);
                    return;
                }
            }

            if (_parsedMsg.TryGetValue(Keys.INSTRUCTION, out string _msgCode))
            {
                DecodeInstruction(_msgCode, _parsedMsg);
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

    private void DecodeInstruction(string _msgCode, Dictionary<string, string> _parsedMsg)
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
            OnAvatarCreated(_avatarInfo);
            // onAvatarCreated?.Invoke(_avatarInfo);
        }
        else if (_msgCode == Instructions.CREATE_WORLD)
        {
            WorldInfo _worldInfo = new WorldInfo()
            {
                worldId = _parsedMsg[Keys.WORLD_ID],
                worldName = _parsedMsg[Keys.WORLD_NAME]
            };
            OnWorldCreated(_worldInfo);
            // onWorldCreated?.Invoke(_worldInfo);
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
            OnAllAvatarsRetrieved(_avatars);
            // onAllAvatarsRetrieved?.Invoke(_avatars);
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
            OnAllWorldsRetrieved(_worlds);
            // onAllWorldsRetrieved?.Invoke(_worlds);
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
            OnWorldJoinned(_joinInfo);
            // onWorldJoined?.Invoke(_joinInfo);
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
            OnAllAvatarsRetrieved(_avatars);
            // onAllAvatarsRetrieved?.Invoke(_avatars);
        }
        else if (_msgCode == Instructions.SEND_MSG)
        {
            Dictionary<string, string> _msgInfo = new Dictionary<string, string>() {
                        {Keys.WORLD_ID, _parsedMsg[Keys.WORLD_ID]},
                        {Keys.AVATAR_ID, _parsedMsg[Keys.AVATAR_ID]},
                        {Keys.MESSAGE, _parsedMsg[Keys.MESSAGE]}
                    };
            OnMessageReceived(_msgInfo);
            // onMessageRecieved?.Invoke(_msgInfo);
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

    private void EncryptMsg(string _message, string _publicKey)
    {
        if (_publicKey == null || _publicKey.Length == 0)
        {
#if UNITY_EDITOR
            Debug.Log("Valid public key is not present.");
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
        m_localServerClient.SendMessageToServer(_msg);
    }

    private void DecryptMsg(Dictionary<string, string> _encryptedMsg)
    {
        if (m_privateKey == null || m_privateKey.Length == 0)
        {
#if UNITY_EDITOR
            Debug.Log("Valid public key is not present.");
#endif
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Decrypting Msg...");
#endif
        Dictionary<string, string> _msgDict = new Dictionary<string, string> {
            {Keys.INSTRUCTION, Instructions.DECRYPT_MSG },
            {Keys.PRIVATE_KEY, m_privateKey}
        };

        foreach (KeyValuePair<string, string> item in _encryptedMsg)
        {
            _msgDict.Add(item.Key, item.Value);
        }

        string _msg = JsonConvert.SerializeObject(_msgDict);
#if UNITY_EDITOR
        Debug.Log(_msg);
#endif
        m_localServerClient.SendMessageToServer(_msg);
    }

    public void LogIn()
    {
        logInProcess.ChangeProcessToCompleted(null);
    }

    public void SetUserId(string _userId)
    {
        m_usedId = _userId;
    }

    public void CreateAvatar(string _avatarName, string _viewId)
    {
        avatarCreationProcess.ChangeProcessToRunning();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.CREATE_AVATAR},
            {Keys.AVATAR_NAME, _avatarName},
            {Keys.AVATAR_VIEW_ID, _viewId}
        };
        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg, m_serverPublicKey);
    }

    public void CreateWorld(string _worldName, string _viewId)
    {
        worldCreationProcess.ChangeProcessToRunning();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.CREATE_WORLD},
            {Keys.WORLD_NAME, _worldName},
            {Keys.WORLD_VIEW_ID, _viewId}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg, m_serverPublicKey);
    }

    public void GetAllMyAvatars()
    {
        getAllAvatarsProcess.ChangeProcessToRunning();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.CLIENT_ALL_AVATARS}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg, m_serverPublicKey);
    }

    public void GetAllWorlds()
    {
        getAllWorldsProcess.ChangeProcessToRunning();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.ALL_WORLDS}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg, m_serverPublicKey);
    }

    public void JoinWorld(string _avatarId, string _worldId)
    {
        worldJoinnedProcess.ChangeProcessToRunning();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.JOIN_WORLD},
            {Keys.AVATAR_ID, _avatarId},
            {Keys.WORLD_ID, _worldId}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg, m_serverPublicKey);
    }

    public void GetAllAvatarsFromWorld(string _worldId)
    {
        getAllAvatarsProcess.ChangeProcessToRunning();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.WORLD_ALL_AVATARS},
            {Keys.WORLD_ID, _worldId}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg, m_serverPublicKey);
    }

    public void SendPublicKeyToServer(string _key)
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.CLIENT_KEY},
            {Keys.PUBLIC_KEY, _key}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg, m_serverPublicKey);
    }

    public void SendMessageToReceiver(string _worldId, string _receiverId, string _sendMsg)
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.SEND_MSG},
            {Keys.WORLD_ID, _worldId},
            {Keys.RECIEVER_ID, _receiverId},
            {Keys.MESSAGE, _sendMsg}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg, m_serverPublicKey);
    }

    public void GenerateKey()
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>
        {
            { Keys.INSTRUCTION, Instructions.GENERATE_KEY }
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
#if UNITY_EDITOR
        Debug.Log(_msg);
#endif
        m_localServerClient.SendMessageToServer(_msg);
    }
}

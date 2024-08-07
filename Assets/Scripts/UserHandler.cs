using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class UserHandler
{
    private string m_privateKey;
    private string m_publicKey;
    private string m_serverPublicKey;
    private string m_userId;
    private bool m_isConnectedToLocalServer;
    private bool m_isConnectedToMainServer;

    public bool IsConnected => m_isConnectedToLocalServer && m_isConnectedToMainServer;
    public string UserId => m_userId;
    public bool IsLoggedIn => !string.IsNullOrEmpty(m_userId) && !string.IsNullOrWhiteSpace(m_userId);

    private readonly Client m_mainServerClient;
    private readonly LocalClient m_localServerClient;

    public CallbackHandler<string> logInCallback;
    public CallbackHandler<string> registrationCallback;
    public CallbackHandler<AvatarInfo> avatarCreationCallback;
    public CallbackHandler<WorldInfo> worldCreationCallback;
    public CallbackHandler<List<AvatarInfo>> getAllAvatarsCallback;
    public CallbackHandler<List<WorldInfo>> getAllWorldsCallback;
    public CallbackHandler<AvatarAndWorldInfo> worldJoinnedCallback;
    public CallbackHandler<Dictionary<string, string>> messageReceivedCallback;

    public UserHandler()
    {
        m_mainServerClient = new Client();
        m_localServerClient = new LocalClient();

        logInCallback = new CallbackHandler<string>();
        registrationCallback = new CallbackHandler<string>();
        avatarCreationCallback = new CallbackHandler<AvatarInfo>();
        worldCreationCallback = new CallbackHandler<WorldInfo>();
        getAllAvatarsCallback = new CallbackHandler<List<AvatarInfo>>();
        getAllWorldsCallback = new CallbackHandler<List<WorldInfo>>();
        worldJoinnedCallback = new CallbackHandler<AvatarAndWorldInfo>();
        messageReceivedCallback = new CallbackHandler<Dictionary<string, string>>();

        m_mainServerClient.onConnectedWithServer += () =>
        {
            m_isConnectedToMainServer = true;
        };

        m_localServerClient.onConnectedWithServer += () =>
        {
            m_isConnectedToLocalServer = true;
        };

        worldJoinnedCallback.onSuccessCallbackDuringUpdateFrame += (_joinInfo) =>
        {
            GameManager.Instance.UpdateSelectedWorld(_joinInfo.worldInfo);
        };

        m_mainServerClient.onMessageReceived += (_msg) =>
        {
            ParseMessage(_msg);
        };

        m_localServerClient.onConnectedWithServer += GenerateKey;

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
        messageReceivedCallback.onSuccessCallbackDuringUpdateFrame += (_) => { };
    }

    public void ConnectMainServer(string _ipAddress, int _port)
    {
        m_mainServerClient.ConnectToServer(_ipAddress, _port);
    }

    public void ConnectLocalServer(string _ipAddress, int _port)
    {
        m_localServerClient.ConnectToServer(_ipAddress, _port);
    }

    public void Disconnect()
    {
        m_mainServerClient?.Disconnect();
        m_localServerClient?.Disconnect();
    }

    public void Update()
    {
        logInCallback.Update();
        registrationCallback.Update();
        avatarCreationCallback.Update();
        worldCreationCallback.Update();
        getAllAvatarsCallback.Update();
        getAllWorldsCallback.Update();
        worldJoinnedCallback.Update();
        messageReceivedCallback.Update();
        messageReceivedCallback.ChangeToPending();
    }

    // private void OnMessageReceived(Dictionary<string, string> _msgInfo)
    // {
    //     messageReceivedCallback.ChangeToSuccess(_msgInfo);
    // }

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
        else if (_msgCode == Instructions.REGISTER_USER)
        {
            if (_parsedMsg.TryGetValue(Keys.ERROR, out string _errorMsg))
            {
#if UNITY_EDITOR
                Debug.Log(_errorMsg);
#endif
                CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg(_msgCode + " -> " + _errorMsg, 1f));
                registrationCallback.ChangeToFailure(_errorMsg);
                return;
            }
            m_userId = _parsedMsg[Keys.USER_ID];
            registrationCallback.ChangeToSuccess(m_userId);
#if UNITY_EDITOR
            Debug.Log("User ID -> " + m_userId);
#endif

        }
        else if (_msgCode == Instructions.LOGIN_USER)
        {
            if (_parsedMsg.TryGetValue(Keys.ERROR, out string _errorMsg))
            {
#if UNITY_EDITOR
                Debug.Log(_errorMsg);
#endif
                CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg(_msgCode + " -> " + _errorMsg, 1f));
                logInCallback.ChangeToFailure(_errorMsg);
                return;
            }
            m_userId = _parsedMsg[Keys.USER_ID];
            logInCallback.ChangeToSuccess(m_userId);
#if UNITY_EDITOR
            Debug.Log("User ID -> " + m_userId);
#endif
        }
        else if (_msgCode == Instructions.CREATE_AVATAR)
        {
            if (_parsedMsg.TryGetValue(Keys.ERROR, out string _errorMsg))
            {
#if UNITY_EDITOR
                Debug.Log(_errorMsg);
#endif
                CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg(_msgCode + " -> " + _errorMsg, 1f));
                avatarCreationCallback.ChangeToFailure(_errorMsg);
                return;
            }

            AvatarInfo _avatarInfo = new AvatarInfo()
            {
                avatarId = _parsedMsg[Keys.AVATAR_ID],
                avatarName = _parsedMsg[Keys.AVATAR_NAME]
            };

            avatarCreationCallback.ChangeToSuccess(_avatarInfo);
        }
        else if (_msgCode == Instructions.CREATE_WORLD)
        {
            if (_parsedMsg.TryGetValue(Keys.ERROR, out string _errorMsg))
            {
#if UNITY_EDITOR
                Debug.Log(_errorMsg);
#endif
                CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg(_msgCode + " -> " + _errorMsg, 1f));
                worldCreationCallback.ChangeToFailure(_errorMsg);
                return;
            }

            WorldInfo _worldInfo = new WorldInfo()
            {
                worldId = _parsedMsg[Keys.WORLD_ID],
                worldName = _parsedMsg[Keys.WORLD_NAME]
            };
            worldCreationCallback.ChangeToSuccess(_worldInfo);
        }
        else if (_msgCode == Instructions.CLIENT_ALL_AVATARS)
        {
            if (_parsedMsg.TryGetValue(Keys.ERROR, out string _errorMsg))
            {
#if UNITY_EDITOR
                Debug.Log(_errorMsg);
#endif
                CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg(_msgCode + " -> " + _errorMsg, 1f));
                getAllAvatarsCallback.ChangeToFailure(_errorMsg);
                return;
            }

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
            getAllAvatarsCallback.ChangeToSuccess(_avatars);
        }
        else if (_msgCode == Instructions.ALL_WORLDS)
        {
            if (_parsedMsg.TryGetValue(Keys.ERROR, out string _errorMsg))
            {
#if UNITY_EDITOR
                Debug.Log(_errorMsg);
#endif
                CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg(_msgCode + " -> " + _errorMsg, 1f));
                getAllWorldsCallback.ChangeToFailure(_errorMsg);
                return;
            }

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
            getAllWorldsCallback.ChangeToSuccess(_worlds);
        }
        else if (_msgCode == Instructions.JOIN_WORLD)
        {
            if (_parsedMsg.TryGetValue(Keys.ERROR, out string _errorMsg))
            {
#if UNITY_EDITOR
                Debug.Log(_errorMsg);
#endif
                CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg(_msgCode + " -> " + _errorMsg, 1f));
                worldJoinnedCallback.ChangeToFailure(_errorMsg);
                return;
            }

            AvatarAndWorldInfo _joinInfo = new AvatarAndWorldInfo()
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
            worldJoinnedCallback.ChangeToSuccess(_joinInfo);
        }
        else if (_msgCode == Instructions.WORLD_ALL_AVATARS)
        {
            if (_parsedMsg.TryGetValue(Keys.ERROR, out string _errorMsg))
            {
#if UNITY_EDITOR
                Debug.Log(_errorMsg);
#endif
                CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg(_msgCode + " -> " + _errorMsg, 1f));
                getAllAvatarsCallback.ChangeToFailure(_errorMsg);
                return;
            }

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
            getAllAvatarsCallback.ChangeToSuccess(_avatars);
        }
        else if (_msgCode == Instructions.SEND_MSG)
        {
            if (_parsedMsg.TryGetValue(Keys.ERROR, out string _errorMsg))
            {
#if UNITY_EDITOR
                Debug.Log(_errorMsg);
#endif
                CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg(_msgCode + " -> " + _errorMsg, 1f));
                messageReceivedCallback.ChangeToFailure(_errorMsg);
                return;
            }

            Dictionary<string, string> _msgInfo = new Dictionary<string, string>() {
                        {Keys.WORLD_ID, _parsedMsg[Keys.WORLD_ID]},
                        {Keys.AVATAR_ID, _parsedMsg[Keys.AVATAR_ID]},
                        {Keys.MESSAGE, _parsedMsg[Keys.MESSAGE]}
                    };

            messageReceivedCallback.ChangeToSuccess(_msgInfo);
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("Message is sent without proper instruction -> " + _msgCode);
#endif
            CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg("Message is sent without proper instruction -> " + _msgCode, 1f));
        }
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

    public void Register(Dictionary<string, string> _infoDict)
    {
        registrationCallback.ChangeToPending();
        string _info = JsonConvert.SerializeObject(_infoDict);
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.REGISTER_USER},
            {Keys.REGISTRATION_INFO, _info}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        EncryptMsg(_msg, m_serverPublicKey);
    }

    public void LogIn(Dictionary<string, string> _infoDict)
    {
        logInCallback.ChangeToPending();
        string _info = JsonConvert.SerializeObject(_infoDict);
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.LOGIN_USER},
            {Keys.LOGIN_INFO, _info}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        EncryptMsg(_msg, m_serverPublicKey);
    }

    public void CreateAvatar(string _avatarName, string _viewId)
    {
        avatarCreationCallback.ChangeToPending();
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
        worldCreationCallback.ChangeToPending();
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
        getAllAvatarsCallback.ChangeToPending();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.CLIENT_ALL_AVATARS}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg, m_serverPublicKey);
    }

    public void GetAllWorlds()
    {
        getAllWorldsCallback.ChangeToPending();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.ALL_WORLDS}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg, m_serverPublicKey);
    }

    public void JoinWorld(string _avatarId, string _worldId)
    {
        worldJoinnedCallback.ChangeToPending();
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
        getAllAvatarsCallback.ChangeToPending();
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
        m_mainServerClient.SendMessageToServer(_msg);
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
        Debug.Log("Generate Public & Private Key...");
        Debug.Log(_msg);
#endif
        m_localServerClient.SendMessageToServer(_msg);
    }
}

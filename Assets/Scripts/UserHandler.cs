using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class UserHandler
{
    // Client keys
    private string m_kyberPrivateKey;
    private string m_kyberPublicKey;
    private string m_dilithiumPrivateKey;
    private string m_dilithiumPublicKey;

    // Server keys
    private string m_serverKyberPublicKey;
    private string m_serverDilithiumPublicKey;

    private string m_userId;
    private bool m_isConnectedToLocalServer;
    private bool m_isConnectedToMainServer;

    public bool IsConnected => m_isConnectedToLocalServer && m_isConnectedToMainServer;
    public string UserId => m_userId;
    public bool IsLoggedIn => !string.IsNullOrEmpty(m_userId) && !string.IsNullOrWhiteSpace(m_userId);

    private readonly ServerClient m_mainServerClient;
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
        m_mainServerClient = new ServerClient();
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
            GetServerPublicKey();
        };

        m_mainServerClient.onMessageReceived += (_msg) =>
        {
            ParseServerMessage(_msg);
        };

        m_localServerClient.onConnectedWithServer += () =>
        {
            m_isConnectedToLocalServer = true;
            GenerateKey();
        };

        m_localServerClient.onMessageReceived += (_msg) =>
        {
            ParseLocalMessage(_msg);
        };

        worldJoinnedCallback.onSuccessCallbackDuringUpdateFrame += (_joinInfo) =>
        {
            GameManager.Instance.UpdateSelectedWorld(_joinInfo.worldInfo);
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

    private void ParseLocalMessage(string _message)
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
                if (_msgCode == LocalInstructions.GENERATE_KEY)
                {
                    ParseKeys(_parsedMsg);
                    SendPublicKeyToServer();
                }
                else if (_msgCode == LocalInstructions.ENCRYPT_MSG)
                {
                    ParseEncryptedMessage(_parsedMsg);
                }
                else if (_msgCode == LocalInstructions.DECRYPT_MSG)
                {
                    ParseDecryptedMessage(_parsedMsg);
                }
                else if(_msgCode == LocalInstructions.SIGN_MSG)
                {
                    ParseSignatureMessage(_parsedMsg);
                }
                else if(_msgCode == LocalInstructions.VERIFY_MSG)
                {
                    ParseVerifiedMessage(_parsedMsg);
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

    private void ParseServerMessage(string _message)
    {
        if (_message.Length == 0)
        {
            return;
        }

        try
        {
            Dictionary<string, string> _parsedMsg = JsonConvert.DeserializeObject<Dictionary<string, string>>(_message);

            // For Digital Signature verify
            if(_parsedMsg.TryGetValue(Keys.SIGNATURE, out string _signature))
            {
                Verify(_parsedMsg[Keys.MESSAGE], _signature);
                return;
            }

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

    private void ParseKeys(Dictionary<string, string> _parsedMsg)
    {
#if UNITY_EDITOR
        Debug.Log("Parsing Public & Private Key...");
#endif

        if (!_parsedMsg.TryGetValue(Keys.KYBER_PUBLIC_KEY, out string _kyberPublicKey))
        {
#if UNITY_EDITOR
            Debug.Log("No Kyber Public Key is given in the msg.");
#endif
            return;
        }

        if (!_parsedMsg.TryGetValue(Keys.KYBER_PRIVATE_KEY, out string _kyberPrivateKey))
        {
#if UNITY_EDITOR
            Debug.Log("No Kyber Private Key is given in the msg.");
#endif
            return;
        }

        if (!_parsedMsg.TryGetValue(Keys.DILITHIUM_PUBLIC_KEY, out string _dilithiumPublicKey))
        {
#if UNITY_EDITOR
            Debug.Log("No Dilithium Public Key is given in the msg.");
#endif
            return;
        }

        if (!_parsedMsg.TryGetValue(Keys.DILITHIUM_PRIVATE_KEY, out string _dilithiumPrivateKey))
        {
#if UNITY_EDITOR
            Debug.Log("No Dilithium Private Key is given in the msg.");
#endif
            return;
        }

        m_kyberPublicKey = _kyberPublicKey;
        m_kyberPrivateKey = _kyberPrivateKey;
        m_dilithiumPublicKey = _dilithiumPublicKey;
        m_dilithiumPrivateKey = _dilithiumPrivateKey;
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
            string encSessionKey = _parsedMsg[Keys.ENCAPSULATED_KEY];
            string tag = _parsedMsg[Keys.TAG];
            string cipherText = _parsedMsg[Keys.CIPHER_TEXT];
            string nonce = _parsedMsg[Keys.NONCE];

            Dictionary<string, string> _encryptedMsg = new Dictionary<string, string>() {
                {Keys.MSG_TYPE, MessageType.ENCRYPTED_TEXT },
                {Keys.ENCAPSULATED_KEY, encSessionKey},
                {Keys.TAG, tag},
                {Keys.CIPHER_TEXT, cipherText},
                {Keys.NONCE, nonce}
            };
#if UNITY_EDITOR && DEBUG
            Debug.Log("Parsing Encrypted Msg Completed...");
#endif
            //SendEncryptedMsgToServer(_encryptedMsg);
            string _msg = JsonConvert.SerializeObject(_encryptedMsg);
            Signature(_msg);
        }
        catch (ArgumentException e)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Error in parsing Encrypted Msg " + e.Message);
            Debug.Log("Parsing Encrypted Msg Failed...");
#endif
        }
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
#if UNITY_EDITOR
        Debug.Log("Parsing Decrypted Msg Completed...");
        Debug.Log(_msg);
#endif
        try
        {
            _parsedMsg = JsonConvert.DeserializeObject<Dictionary<string, string>>(_msg);
            if (_parsedMsg.TryGetValue(Keys.INSTRUCTION, out string _msgCode))
            {
                DecodeInstruction(_msgCode, _parsedMsg);
            }
#if UNITY_EDITOR
            else
            {
                Debug.Log("No instruction is given with the msg...");
                Debug.Log(_msg);
            }
#endif
        }
        catch (JsonReaderException e)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Json deserializatio error : " + e.ToString());
#endif
        }
#if UNITY_EDITOR
        Debug.Log("Decrypting Msg Completed...");
#endif
    }

    private void ParseSignatureMessage(Dictionary<string, string> _parsedMsg)
    {
#if UNITY_EDITOR
        Debug.Log("Parsing Signature Msg...");
#endif

        string _msg = _parsedMsg[Keys.MESSAGE];
        string _sign = _parsedMsg[Keys.SIGNATURE];

        Dictionary<string, string> _info = new Dictionary<string, string>
        {
            {Keys.SIGNATURE, _sign},
            {Keys.MESSAGE, _msg},
        };

        _msg = JsonConvert.SerializeObject(_info);
        m_mainServerClient.SendMessageToServer(_msg);
    }

    private void ParseVerifiedMessage(Dictionary<string, string> _parsedMsg)
    {
#if UNITY_EDITOR
        Debug.Log("Parsing Vefifying Msg...");
#endif
        string status = _parsedMsg[Keys.VERIFICATION_STATUS];

        if (status.CompareTo(VerificationStatus.VERIFIED) == 0)
        {
#if UNITY_EDITOR
            Debug.Log("Verification success...");
#endif
            string _msg = _parsedMsg[Keys.MESSAGE];

            _parsedMsg = JsonConvert.DeserializeObject<Dictionary<string, string>>(_msg);

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
                if (_parsedMsg.TryGetValue(Keys.MESSAGE, out _msg))
                {
                    Debug.Log(_msg);
                }
            }
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("Verification failed...");
#endif
        }
    }

    private void DecodeInstruction(string _msgCode, Dictionary<string, string> _parsedMsg)
    {
        if (_msgCode == ServerInstructions.GET_SERVER_KEY)
        {
            m_serverKyberPublicKey = _parsedMsg[Keys.KYBER_PUBLIC_KEY];
            m_serverDilithiumPublicKey = _parsedMsg[Keys.DILITHIUM_PUBLIC_KEY];
#if UNITY_EDITOR
            Debug.Log("Server Kyber Public Key -> " + m_serverKyberPublicKey.Truncate(50));
            Debug.Log("Server Dilithium Public Key -> " + m_serverDilithiumPublicKey.Truncate(50));
            if (_parsedMsg.TryGetValue(Keys.MESSAGE, out string _msg))
            {
                Debug.Log(_msg);
            }
#endif
        }
        else if (_msgCode == ServerInstructions.REGISTER_USER)
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
        else if (_msgCode == ServerInstructions.LOGIN_USER)
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
        else if (_msgCode == ServerInstructions.CREATE_AVATAR)
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
#if UNITY_EDITOR
            Debug.Log("New Avatar is created with id : " + _avatarInfo.avatarId);
#endif
        }
        else if (_msgCode == ServerInstructions.CREATE_WORLD)
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
        else if (_msgCode == ServerInstructions.GET_CLIENT_ALL_AVATARS)
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
        else if (_msgCode == ServerInstructions.GET_ALL_WORLDS)
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
        else if (_msgCode == ServerInstructions.JOIN_WORLD)
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
        else if (_msgCode == ServerInstructions.GET_WORLD_ALL_AVATARS)
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
        else if (_msgCode == ServerInstructions.SEND_MSG)
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

    private void GenerateKey()
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>
        {
            { Keys.INSTRUCTION, LocalInstructions.GENERATE_KEY }
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
#if UNITY_EDITOR
        Debug.Log("Generate Public & Private Key...");
        Debug.Log(_msg);
#endif
        m_localServerClient.SendMessageToServer(_msg);
    }

    private void EncryptMsg(string _message)
    {
        if (m_serverKyberPublicKey == null || m_serverKyberPublicKey.Length == 0)
        {
#if UNITY_EDITOR
            Debug.Log("Valid server kyber public key is not present.");
#endif
            return;
        }

#if UNITY_EDITOR
        Debug.Log("Encrypting Msg...");
#endif
        Dictionary<string, string> _msgDict = new Dictionary<string, string>
        {
            { Keys.INSTRUCTION, LocalInstructions.ENCRYPT_MSG },
            { Keys.KYBER_PUBLIC_KEY, m_serverKyberPublicKey },
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
        if (m_kyberPrivateKey == null || m_kyberPrivateKey.Length == 0)
        {
#if UNITY_EDITOR
            Debug.Log("Valid Kyber public key is not present.");
#endif
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Decrypting Msg...");
#endif
        Dictionary<string, string> _msgDict = new Dictionary<string, string> {
            {Keys.INSTRUCTION, LocalInstructions.DECRYPT_MSG },
            {Keys.KYBER_PRIVATE_KEY, m_kyberPrivateKey}
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

    private void Signature(string _msg)
    {
        if (m_dilithiumPrivateKey == null || m_dilithiumPrivateKey.Length == 0)
        {
#if UNITY_EDITOR
            Debug.Log("Valid Dilithium private key is not present.");
#endif
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Signing Msg...");
#endif
        Dictionary<string, string> _info = new Dictionary<string, string>
        {
            { Keys.INSTRUCTION, LocalInstructions.SIGN_MSG },
            { Keys.DILITHIUM_PRIVATE_KEY, m_dilithiumPrivateKey},
            { Keys.MESSAGE, _msg }
        };

        m_localServerClient.SendMessageToServer(JsonConvert.SerializeObject(_info));
    }

    private void Verify(string _msg, string _sign)
    {
        if (m_serverDilithiumPublicKey == null || m_serverDilithiumPublicKey.Length == 0)
        {
#if UNITY_EDITOR
            Debug.Log("Valid Dilithium private key is not present.");
#endif
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Verifying Msg...");
#endif

        Dictionary<string, string> _info = new Dictionary<string, string>
        {
            { Keys.INSTRUCTION, LocalInstructions.VERIFY_MSG},
            { Keys.DILITHIUM_PUBLIC_KEY, m_serverDilithiumPublicKey },
            { Keys.SIGNATURE, _sign },
            { Keys.MESSAGE, _msg }
        };

        m_localServerClient.SendMessageToServer(JsonConvert.SerializeObject(_info));
    }

    private void GetServerPublicKey()
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>
        {
            { Keys.INSTRUCTION, ServerInstructions.GET_SERVER_KEY },
            { Keys.MSG_TYPE, MessageType.PLAIN_TEXT }
        };

#if UNITY_EDITOR
        Debug.Log("Get Server Public Key...");
#endif

        string _msg = JsonConvert.SerializeObject(_msgDict);
        m_mainServerClient.SendMessageToServer(_msg);
    }

    public void Register(Dictionary<string, string> _infoDict)
    {
        registrationCallback.ChangeToPending();
        string _info = JsonConvert.SerializeObject(_infoDict);
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, ServerInstructions.REGISTER_USER},
            {Keys.REGISTRATION_INFO, _info}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        EncryptMsg(_msg);
    }

    public void LogIn(Dictionary<string, string> _infoDict)
    {
        logInCallback.ChangeToPending();
        string _info = JsonConvert.SerializeObject(_infoDict);
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, ServerInstructions.LOGIN_USER},
            {Keys.LOGIN_INFO, _info}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        EncryptMsg(_msg);
    }

    public void CreateAvatar(string _avatarName, string _viewId)
    {
        avatarCreationCallback.ChangeToPending();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, ServerInstructions.CREATE_AVATAR},
            {Keys.AVATAR_NAME, _avatarName},
            {Keys.AVATAR_VIEW_ID, _viewId}
        };
        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg);
    }

    public void CreateWorld(string _worldName, string _viewId)
    {
        worldCreationCallback.ChangeToPending();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, ServerInstructions.CREATE_WORLD},
            {Keys.WORLD_NAME, _worldName},
            {Keys.WORLD_VIEW_ID, _viewId}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg);
    }

    public void GetAllMyAvatars()
    {
        getAllAvatarsCallback.ChangeToPending();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, ServerInstructions.GET_CLIENT_ALL_AVATARS}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg);
    }

    public void GetAllWorlds()
    {
        getAllWorldsCallback.ChangeToPending();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, ServerInstructions.GET_ALL_WORLDS}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg);
    }

    public void JoinWorld(string _avatarId, string _worldId)
    {
        worldJoinnedCallback.ChangeToPending();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, ServerInstructions.JOIN_WORLD},
            {Keys.AVATAR_ID, _avatarId},
            {Keys.WORLD_ID, _worldId}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg);
    }

    public void GetAllAvatarsFromWorld(string _worldId)
    {
        getAllAvatarsCallback.ChangeToPending();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, ServerInstructions.GET_WORLD_ALL_AVATARS},
            {Keys.WORLD_ID, _worldId}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg);
    }

    public void SendPublicKeyToServer()
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, ServerInstructions.SET_CLIENT_KEY},
            {Keys.KYBER_PUBLIC_KEY, m_kyberPublicKey },
            {Keys.DILITHIUM_PUBLIC_KEY, m_dilithiumPublicKey }
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        m_mainServerClient.SendMessageToServer(_msg);
    }

    //public void SendEncryptedMsgToServer(Dictionary<string, string> _encryptedMsg)
    //{
    //    _encryptedMsg[Keys.MSG_TYPE] = MessageType.ENCRYPTED_TEXT;
    //    string _msg = JsonConvert.SerializeObject(_encryptedMsg);
    //    m_mainServerClient.SendMessageToServer(_msg);
    //}

    public void SendSignatureMsgToServer(Dictionary<string, string> _encryptedMsg)
    {

    }

    public void SendMessageToReceiver(string _worldId, string _receiverId, string _sendMsg)
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, ServerInstructions.SEND_MSG},
            {Keys.WORLD_ID, _worldId},
            {Keys.RECIEVER_ID, _receiverId},
            {Keys.MESSAGE, _sendMsg}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        // m_mainServerClient.SendMessageToServer(_msg);
        EncryptMsg(_msg);
    }
}
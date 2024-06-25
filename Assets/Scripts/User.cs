using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Client))]
public class User : MonoBehaviour
{
    [SerializeField]
    private string m_usedId;

    private string m_privateKey;
    private string m_publicKey;

    private Client m_mainServerClient;
    private LocalClient m_localServerClient;

    public string UserId => m_usedId;

    public ProcessHandler<AvatarInfo> logInProcess;
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

        m_localServerClient.onKeyGenerated += (pk, sk) =>
        {
            m_publicKey = pk;
            m_privateKey = sk;
            SendPublicKeyToServer(m_publicKey);
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

        m_mainServerClient.onAvatarCreated += OnAvatarCreated;
        m_mainServerClient.onWorldCreated += OnWorldCreated;
        m_mainServerClient.onAllAvatarsRetrieved += OnAllAvatarsRetrieved;
        m_mainServerClient.onAllWorldsRetrieved += OnAllWorldsRetrieved;
        m_mainServerClient.onWorldJoined += OnWorldJoinned;
        m_mainServerClient.onMessageRecieved += OnMessageReceived;
    }

    private void OnDisable()
    {
        messageReceivedProcess.Unsubscribe(GameManager.Instance.OnMessageReceived);

        m_mainServerClient.onAvatarCreated -= OnAvatarCreated;
        m_mainServerClient.onWorldCreated -= OnWorldCreated;
        m_mainServerClient.onAllAvatarsRetrieved -= OnAllAvatarsRetrieved;
        m_mainServerClient.onAllWorldsRetrieved -= OnAllWorldsRetrieved;
        m_mainServerClient.onWorldJoined -= OnWorldJoinned;
        m_mainServerClient.onMessageRecieved -= OnMessageReceived;
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

    public void logIn()
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
        m_mainServerClient.SendMessageToServer(_msg);
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
        m_mainServerClient.SendMessageToServer(_msg);
    }

    public void GetAllMyAvatars()
    {
        getAllAvatarsProcess.ChangeProcessToRunning();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.CLIENT_ALL_AVATARS}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        m_mainServerClient.SendMessageToServer(_msg);
    }

    public void GetAllWorlds()
    {
        getAllWorldsProcess.ChangeProcessToRunning();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.ALL_WORLDS}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        m_mainServerClient.SendMessageToServer(_msg);
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
        m_mainServerClient.SendMessageToServer(_msg);
    }

    public void GetAllAvatarsFromWorld(string _worldId)
    {
        getAllAvatarsProcess.ChangeProcessToRunning();
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.WORLD_ALL_AVATARS},
            {Keys.WORLD_ID, _worldId}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        m_mainServerClient.SendMessageToServer(_msg);
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
        m_mainServerClient.SendMessageToServer(_msg);
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

    public void EncryptMsg(string _message)
    {
        if (m_publicKey == null || m_publicKey.Length == 0)
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
            { Keys.PUBLIC_KEY, m_publicKey },
            { Keys.MESSAGE, _message }
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
#if UNITY_EDITOR
        Debug.Log(_msg);
#endif
        m_localServerClient.SendMessageToServer(_msg);
    }

    public void DecryptMsg(Dictionary<string, string> _encryptedMsg)
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
}

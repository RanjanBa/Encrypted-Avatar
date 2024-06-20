using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using TMPro;
using DigitalMetaverse;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get { return m_instance; } }

    private static GameManager m_instance;

    [SerializeField]
    private User m_userPrefab;
    [SerializeField]
    private List<IconWithID> m_defaultAvatars = new List<IconWithID>();
    [SerializeField]
    private List<IconWithID> m_defaultWorlds = new List<IconWithID>();

    private User m_selectedUser;
    private List<User> m_users = new List<User>();
    private AvatarInfo m_selectedAvatar;
    private WorldInfo m_selectedWorld;

    public ProcessHandler<AvatarInfo> logInProcess;
    public ProcessHandler<AvatarInfo> avatarCreationProcess;
    public ProcessHandler<WorldInfo> worldCreationProcess;
    public ProcessHandler<List<AvatarInfo>> getAllAvatarsProcess;
    public ProcessHandler<List<WorldInfo>> getAllWorldsProcess;
    public ProcessHandler<JoinInfo> worldJoinnedProcess;
    public Action<WorldInfo> onSelectedWorldChanged;

    public ReadOnlyCollection<IconWithID> DefaultAvatars => new ReadOnlyCollection<IconWithID>(m_defaultAvatars);
    public ReadOnlyCollection<IconWithID> DefaultWorlds => new ReadOnlyCollection<IconWithID>(m_defaultWorlds);

    private void Awake()
    {
        if (m_instance != null)
        {
            if (m_instance.gameObject == gameObject)
            {
                Destroy(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        m_instance = this;
        logInProcess = new ProcessHandler<AvatarInfo>();
        avatarCreationProcess = new ProcessHandler<AvatarInfo>();
        worldCreationProcess = new ProcessHandler<WorldInfo>();
        getAllAvatarsProcess = new ProcessHandler<List<AvatarInfo>>();
        getAllWorldsProcess = new ProcessHandler<List<WorldInfo>>();
        worldJoinnedProcess = new ProcessHandler<JoinInfo>();
    }

    private void Start()
    {
        worldJoinnedProcess.Subscribe((_info) =>
        {
            UpdateSelectedWorld(_info.worldInfo);
        });
    }

    private void Update()
    {
        logInProcess.UpdateProcess();
        avatarCreationProcess.UpdateProcess();
        worldCreationProcess.UpdateProcess();
        getAllAvatarsProcess.UpdateProcess();
        getAllWorldsProcess.UpdateProcess();
        worldJoinnedProcess.UpdateProcess();
    }

    public Sprite GetAvatarSprite(string _viewId)
    {
        foreach (var _avatar in m_defaultAvatars)
        {
            if (_viewId == _avatar.viewId)
            {
                return _avatar.icon;
            }
        }

        return null;
    }

    public Sprite GetWorldSprite(string _viewId)
    {
        foreach (var _world in m_defaultWorlds)
        {
            if (_viewId == _world.viewId)
            {
                return _world.icon;
            }
        }

        return null;
    }

    public void UpdateSelectedAvatar(AvatarInfo _avatarInfo)
    {
        m_selectedAvatar = _avatarInfo;
    }

    public void UpdateSelectedWorld(WorldInfo _worldInfo)
    {
        m_selectedWorld = _worldInfo;
        onSelectedWorldChanged?.Invoke(_worldInfo);
    }

        public void SignIn()
        {
            User _user = Instantiate(m_userPrefab);
            m_selectedUser = _user;
            m_users.Add(_user);
            logInProcess.ChangeProcessToCompleted(null);
        }

        public void SignUp()
        {
            User _user = Instantiate(m_userPrefab);
            m_selectedUser = _user;
            m_users.Add(_user);
            logInProcess.ChangeProcessToCompleted(null);
        }

    public void CreateAvatar(string _avatarName, string _viewId)
    {
        if (m_selectedUser == null)
        {
#if UNITY_EDITOR
            Debug.Log("No User is selected. First SignIn or SignUp.");
#endif
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Creating Avatar -> " + _avatarName + "...");
#endif
        avatarCreationProcess.ChangeProcessToRunning();
        m_selectedUser.CreateAvatar(_avatarName, _viewId);
    }

    public void CreateWorld(string _worldName, string _viewId)
    {
        if (m_selectedUser == null)
        {
#if UNITY_EDITOR
            Debug.Log("No User is selected. First SignIn or SignUp.");
#endif
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Creating World -> " + _worldName + "...");
#endif
        worldCreationProcess.ChangeProcessToRunning();
        m_selectedUser.CreateWorld(_worldName, _viewId);
    }

    public void JoinWorld()
    {
        if (m_selectedUser == null)
        {
#if UNITY_EDITOR
            Debug.Log("No User is selected. First SignIn or SignUp.");
#endif
            return;
        }
        if (m_selectedAvatar == null)
        {
#if UNITY_EDITOR
            Debug.Log("No Avatar is selected. First Select Any Avatar.");
#endif
            return;
        }

        if (m_selectedAvatar == null)
        {
#if UNITY_EDITOR
            Debug.Log("No World is selected. First Select Any World.");
#endif
            return;
        }

#if UNITY_EDITOR
        Debug.Log("Joining avatar -> " + m_selectedAvatar.avatarName + ", " + "World -> " + m_selectedWorld.worldName + "...");
#endif
        worldJoinnedProcess.ChangeProcessToRunning();
        m_selectedUser.JoinWorld(m_selectedAvatar.avatarId, m_selectedWorld.worldId);
    }

    public void GetAllMyAvatars()
    {
        if (m_selectedUser == null)
        {
#if UNITY_EDITOR
            Debug.Log("No User is selected. First SignIn or SignUp.");
#endif
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Getting all Avatars...");
#endif
        getAllAvatarsProcess.ChangeProcessToRunning();
        m_selectedUser.GetAllMyAvatars();
    }

    public void GetAllWorlds()
    {
        if (m_selectedUser == null)
        {
#if UNITY_EDITOR
            Debug.Log("No User is selected. First SignIn or SignUp.");
#endif
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Getting all Worlds...");
#endif
        getAllWorldsProcess.ChangeProcessToRunning();
        m_selectedUser.GetAllWorlds();
    }

    public void GetAllAvatarsFromSelectedWorld()
    {
        if (m_selectedUser == null)
        {
#if UNITY_EDITOR
            Debug.Log("No User is selected. First SignIn or SignUp.");
#endif
            return;
        }
        if (m_selectedWorld == null)
        {
#if UNITY_EDITOR
            Debug.Log("You have't select any world...");
#endif
            return;
        }

        string _worldId = m_selectedWorld.worldId;
#if UNITY_EDITOR
        Debug.Log("Getting all Avatars from world -> " + _worldId + "...");
#endif
        getAllAvatarsProcess.ChangeProcessToRunning();
        m_selectedUser.GetAllAvatarsFromWorld(_worldId);
    }

    public void AvatarCreationCompleted(AvatarInfo _info)
    {
        avatarCreationProcess.ChangeProcessToCompleted(_info);
    }

    public void WorldCreationCompleted(WorldInfo _info)
    {
        worldCreationProcess.ChangeProcessToCompleted(_info);
    }

    public void WorldJoinnedCompleted(JoinInfo _info)
    {
        worldJoinnedProcess.ChangeProcessToCompleted(_info);
    }

    public void GetAllAvatarsCompleted(List<AvatarInfo> _infos)
    {
        getAllAvatarsProcess.ChangeProcessToCompleted(_infos);
    }

    public void GetAllWorldsCompleted(List<WorldInfo> _infos)
    {
        getAllWorldsProcess.ChangeProcessToCompleted(_infos);
    }
}

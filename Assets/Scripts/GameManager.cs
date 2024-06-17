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
    private World m_selectedWorld;
    private List<User> m_users = new List<User>();
    private List<World> m_worlds = new List<World>();

    public ProcessHandler<AvatarInfo> logInProcess;
    public ProcessHandler<AvatarInfo> avatarCreationProcess;
    public ProcessHandler<WorldInfo> worldCreationProcess;
    public ProcessHandler<List<AvatarInfo>> getAllAvatarsProcess;
    public ProcessHandler<List<WorldInfo>> getAllWorldsProcess;
    public ProcessHandler<List<string>> worldJoinnedProcess;

    public ReadOnlyCollection<IconWithID> DefaultAvatars => new ReadOnlyCollection<IconWithID>(m_defaultAvatars);
    public ReadOnlyCollection<IconWithID> DefaultWorlds => new ReadOnlyCollection<IconWithID>(m_defaultWorlds);

    public AvatarInfo SelectedAvatarInfo { get; set; }
    public WorldInfo SelectedWorldInfo { get; set; }

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
        worldJoinnedProcess = new ProcessHandler<List<string>>();
    }

    private void Start()
    {
        worldJoinnedProcess.onProcessCompleted += OnAvatarJoinnedWorld;
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

    private void OnAvatarJoinnedWorld(List<string> _info)
    {
        // m_selectedWorld = Instantiate()
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
        m_selectedUser.CreateAvatar(_avatarName, _viewId);
        avatarCreationProcess.ChangeProcessToRunning();
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
        m_selectedUser.CreateWorld(_worldName, _viewId);
        worldCreationProcess.ChangeProcessToRunning();
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
        if (SelectedAvatarInfo == null)
        {
#if UNITY_EDITOR
            Debug.Log("No Avatar is selected. First Select Any Avatar.");
#endif
            return;
        }

        if (SelectedWorldInfo == null)
        {
#if UNITY_EDITOR
            Debug.Log("No World is selected. First Select Any World.");
#endif
            return;
        }

#if UNITY_EDITOR
        Debug.Log("Joining avatar -> " + SelectedAvatarInfo.avatarName + ", " + "World -> " + SelectedWorldInfo.worldName + "...");
#endif
        m_selectedUser.JoinWorld(SelectedAvatarInfo.avatarId, SelectedWorldInfo.worldId);
        worldJoinnedProcess.ChangeProcessToRunning();
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
        m_selectedUser.GetAllMyAvatars();
        getAllAvatarsProcess.ChangeProcessToRunning();
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
        m_selectedUser.GetAllWorlds();
        getAllWorldsProcess.ChangeProcessToRunning();
    }

    public void GetAllAvatarsFromWorld(string _worldId)
    {
        if (m_selectedUser == null)
        {
#if UNITY_EDITOR
            Debug.Log("No User is selected. First SignIn or SignUp.");
#endif
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Getting all Avatars from world -> " + _worldId + "...");
#endif
        m_selectedUser.GetAllAvatarsFromWorld(_worldId);
        getAllAvatarsProcess.ChangeProcessToRunning();
    }

    public void AvatarCreationCompleted(AvatarInfo _info)
    {
        avatarCreationProcess.ChangeProcessToCompleted(_info);
    }

    public void WorldCreationCompleted(WorldInfo _info)
    {
        worldCreationProcess.ChangeProcessToCompleted(_info);
    }

    public void WorldJoinnedCompleted(List<string> _infos)
    {
        worldJoinnedProcess.ChangeProcessToCompleted(_infos);
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

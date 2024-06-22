using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

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

    private Transform m_usersContainer;

    private readonly List<User> m_users = new List<User>();

    private User m_selectedUser;
    private AvatarInfo m_selectedAvatar;
    private WorldInfo m_selectedWorld;

    public Action<AvatarInfo> onLoggedIn;
    public Action<AvatarInfo> onAvatarCreated;
    public Action<WorldInfo> onWorldCreated;
    public Action<JoinInfo> onWorldJoinned;

    public Action<WorldInfo> onSelectedWorldChanged;
    public Action<User> onSelectedUserChanged;
    public Action<Dictionary<string, string>> onMessageReceived;

    public ReadOnlyCollection<IconWithID> DefaultAvatars => new ReadOnlyCollection<IconWithID>(m_defaultAvatars);
    public ReadOnlyCollection<IconWithID> DefaultWorlds => new ReadOnlyCollection<IconWithID>(m_defaultWorlds);
    public ReadOnlyCollection<User> Users => new ReadOnlyCollection<User>(m_users);

    public User CurrentlySelectedUser => m_selectedUser;

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
        m_usersContainer = new GameObject("Root").transform;
    }

    private void OnLoggedIn(AvatarInfo _info)
    {
        onLoggedIn?.Invoke(_info);
    }

    private void OnAvatarCreated(AvatarInfo _info)
    {
        onAvatarCreated?.Invoke(_info);
    }

    private void OnWorldCreated(WorldInfo _info)
    {
        onWorldCreated?.Invoke(_info);
    }

    private void OnWorldJoinned(JoinInfo _info)
    {
        onWorldJoinned?.Invoke(_info);
    }

    public void OnMessageReceived(Dictionary<string, string> _msgInfo)
    {
        if (m_selectedWorld == null)
        {
#if UNITY_EDITOR
            Debug.Log("Currently none of the world is selected.");
#endif
            return;
        }
        if (_msgInfo[Keys.WORLD_ID] != m_selectedWorld.worldId)
        {
#if UNITY_EDITOR
            Debug.Log("Received world id and currently selected world id is not equal");
#endif
            return;
        }
        onMessageReceived?.Invoke(_msgInfo);
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

    public void UpdateSelectedUser(User _user)
    {
        if (_user == null) return;

        if (m_selectedUser != null)
        {
            m_selectedUser.gameObject.SetActive(false);
            m_selectedUser.logInProcess.Unsubscribe(OnLoggedIn);
            m_selectedUser.avatarCreationProcess.Unsubscribe(OnAvatarCreated);
            m_selectedUser.worldCreationProcess.Unsubscribe(OnWorldCreated);
            m_selectedUser.worldJoinnedProcess.Unsubscribe(OnWorldJoinned);
        }

        _user.gameObject.SetActive(true);
        m_selectedUser = _user;
        onSelectedUserChanged?.Invoke(_user);
        m_selectedUser.logInProcess.Subscribe(OnLoggedIn);
        m_selectedUser.avatarCreationProcess.Subscribe(OnAvatarCreated);
        m_selectedUser.worldCreationProcess.Subscribe(OnWorldCreated);
        m_selectedUser.worldJoinnedProcess.Subscribe(OnWorldJoinned);
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

    public void SignIn(User _user)
    {
        if (!m_users.Contains(_user))
        {
            return;
        }
        _user.logIn();
        onLoggedIn?.Invoke(null); // TODO:: change during authentication implementation
        UpdateSelectedUser(_user);
    }

    public void SignUp()
    {
        User _user = Instantiate(m_userPrefab, m_usersContainer);
        _user.gameObject.name = "user " + (m_users.Count + 1).ToString();
        Guid _guid = Guid.NewGuid();
        _user.SetUserId(_guid.ToString());
        _user.logIn();
        UpdateSelectedUser(_user);
        m_users.Add(_user);
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
        m_selectedUser.GetAllAvatarsFromWorld(_worldId);
    }

    public void SendMessageToReceiver(string _receiverId, string _msg)
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
#if UNITY_EDITOR
        Debug.Log("Sending msg to receiver -> " + _receiverId + " within the world -> " + m_selectedWorld.worldId + "...");
#endif
        m_selectedUser.SendMessageToReceiver(m_selectedWorld.worldId, _receiverId, _msg);
    }
}

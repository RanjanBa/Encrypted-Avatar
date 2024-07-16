using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get { return m_instance; } }

    private static GameManager m_instance;

    [SerializeField]
    private string m_serverIpAddress = "127.0.0.1";
    [SerializeField]
    private int m_serverPort = 12000;
    [SerializeField]
    private string m_localIpAddress = "127.0.0.1";
    [SerializeField]
    private int m_localPort = 9000;
    [SerializeField]
    private User m_userPrefab;
    [SerializeField]
    private List<IconWithID> m_defaultAvatars = new List<IconWithID>();
    [SerializeField]
    private List<IconWithID> m_defaultWorlds = new List<IconWithID>();

    private Transform m_usersContainer;
    private List<User> m_registeredUsers;

    private User m_selectedUser;
    private AvatarInfo m_selectedAvatar;
    private WorldInfo m_selectedWorld;

    public Action<string> onUserLoggedIn;
    public Action<string> onUserRegistered;
    public Action<AvatarInfo> onAvatarCreated;
    public Action<WorldInfo> onWorldCreated;
    public Action<AvatarAndWorldInfo> onWorldJoinned;

    public Action<User> onSelectedUserChanged;
    public Action<WorldInfo> onSelectedWorldChanged;
    public Action<Dictionary<string, string>> onMessageReceived;

    public ReadOnlyCollection<IconWithID> DefaultAvatars => new ReadOnlyCollection<IconWithID>(m_defaultAvatars);
    public ReadOnlyCollection<IconWithID> DefaultWorlds => new ReadOnlyCollection<IconWithID>(m_defaultWorlds);

    public User CurrentlySelectedUser
    {
        get { return m_selectedUser; }
        set
        {
            onSelectedUserChanged?.Invoke(value);
            m_selectedUser = value;
        }
    }

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
    }

    private void Start()
    {
        m_usersContainer = new GameObject("Root").transform;
    }

    private void OnAvatarCreated(AvatarInfo _info)
    {
        onAvatarCreated?.Invoke(_info);
    }

    private void OnWorldCreated(WorldInfo _info)
    {
        onWorldCreated?.Invoke(_info);
    }

    private void OnWorldJoinned(AvatarAndWorldInfo _info)
    {
        onWorldJoinned?.Invoke(_info);
    }

    public void AddRegisteredUser(UserHandler _userHandler)
    {
        m_registeredUsers ??= new List<User>();
        User _user = Instantiate(m_userPrefab, m_usersContainer);
        _user.AssignUserHandler(_userHandler);
        m_registeredUsers.Add(_user);
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
        if (_user == CurrentlySelectedUser || _user == null) return;

        if (CurrentlySelectedUser != null)
        {
            CurrentlySelectedUser.UserHandler.avatarCreationCallback.onSuccessCallbackDuringUpdateFrame += OnAvatarCreated;
            CurrentlySelectedUser.UserHandler.worldCreationCallback.onSuccessCallbackDuringUpdateFrame += OnWorldCreated;
            CurrentlySelectedUser.UserHandler.worldJoinnedCallback.onSuccessCallbackDuringUpdateFrame += OnWorldJoinned;
        }

        CurrentlySelectedUser = _user;
        CurrentlySelectedUser.UserHandler.avatarCreationCallback.onSuccessCallbackDuringUpdateFrame -= OnAvatarCreated;
        CurrentlySelectedUser.UserHandler.worldCreationCallback.onSuccessCallbackDuringUpdateFrame -= OnWorldCreated;
        CurrentlySelectedUser.UserHandler.worldJoinnedCallback.onSuccessCallbackDuringUpdateFrame -= OnWorldJoinned;
        onSelectedUserChanged?.Invoke(CurrentlySelectedUser);
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

    public void ConnectUserHandlerWithServer(UserHandler _userHandler)
    {
        _userHandler.ConnectLocalServer(m_localIpAddress, m_localPort);
        _userHandler.ConnectMainServer(m_serverIpAddress, m_serverPort);
    }

    public void RegisterNewUser(string _firstName, string _lastName, string _userName, string _password, UserHandler _userHandler)
    {
        Dictionary<string, string> _info = new Dictionary<string, string>
        {
            [Keys.FIRST_NAME] = _firstName,
            [Keys.LAST_NAME] = _lastName,
            [Keys.USER_NAME] = _userName,
            [Keys.PASSWORD] = _password
        };
        _userHandler.Register(_info);
    }

    public void LogIn(string _userName, string _password, UserHandler _userHandler)
    {
        Dictionary<string, string> _info = new Dictionary<string, string>
        {
            [Keys.USER_NAME] = _userName,
            [Keys.PASSWORD] = _password
        };

        _userHandler.LogIn(_info);
    }

    public void LogOut()
    {
        CurrentlySelectedUser = null;
    }

    public void CreateAvatar(string _avatarName, string _viewId)
    {
        if (CurrentlySelectedUser == null)
        {
#if UNITY_EDITOR
            Debug.Log("No User is selected. First SignIn or SignUp.");
#endif
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Creating Avatar -> " + _avatarName + "...");
#endif
        CurrentlySelectedUser.UserHandler.CreateAvatar(_avatarName, _viewId);
    }

    public void CreateWorld(string _worldName, string _viewId)
    {
        if (CurrentlySelectedUser == null)
        {
#if UNITY_EDITOR
            Debug.Log("No User is selected. First SignIn or SignUp.");
#endif
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Creating World -> " + _worldName + "...");
#endif
        CurrentlySelectedUser.UserHandler.CreateWorld(_worldName, _viewId);
    }

    public void JoinWorld()
    {
        if (CurrentlySelectedUser == null)
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
        CurrentlySelectedUser.UserHandler.JoinWorld(m_selectedAvatar.avatarId, m_selectedWorld.worldId);
    }

    public void GetAllMyAvatars()
    {
        if (CurrentlySelectedUser == null)
        {
#if UNITY_EDITOR
            Debug.Log("No User is selected. First SignIn or SignUp.");
#endif
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Getting all Avatars...");
#endif
        CurrentlySelectedUser.UserHandler.GetAllMyAvatars();
    }

    public void GetAllWorlds()
    {
        if (CurrentlySelectedUser == null)
        {
#if UNITY_EDITOR
            Debug.Log("No User is selected. First SignIn or SignUp.");
#endif
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Getting all Worlds...");
#endif
        CurrentlySelectedUser.UserHandler.GetAllWorlds();
    }

    public void GetAllAvatarsFromSelectedWorld()
    {
        if (CurrentlySelectedUser == null)
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
        CurrentlySelectedUser.UserHandler.GetAllAvatarsFromWorld(_worldId);
    }

    public void SendMessageToReceiver(string _receiverId, string _msg)
    {
        if (CurrentlySelectedUser == null)
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
        CurrentlySelectedUser.UserHandler.SendMessageToReceiver(m_selectedWorld.worldId, _receiverId, _msg);
    }
}

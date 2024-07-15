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
    private List<IconWithID> m_defaultAvatars = new List<IconWithID>();
    [SerializeField]
    private List<IconWithID> m_defaultWorlds = new List<IconWithID>();

    private User m_selectedUser;
    private AvatarInfo m_selectedAvatar;
    private WorldInfo m_selectedWorld;

    public Action<string> onLoggedIn;
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

    private void OnLoggedIn(string _userId)
    {
        onLoggedIn?.Invoke(_userId);
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
            CurrentlySelectedUser.gameObject.SetActive(false);
            CurrentlySelectedUser.logInProcess.Unsubscribe(OnLoggedIn);
            CurrentlySelectedUser.avatarCreationProcess.Unsubscribe(OnAvatarCreated);
            CurrentlySelectedUser.worldCreationProcess.Unsubscribe(OnWorldCreated);
            CurrentlySelectedUser.worldJoinnedProcess.Unsubscribe(OnWorldJoinned);
        }

        _user.gameObject.SetActive(true);
        CurrentlySelectedUser = _user;
        CurrentlySelectedUser.logInProcess.Subscribe(OnLoggedIn);
        CurrentlySelectedUser.avatarCreationProcess.Subscribe(OnAvatarCreated);
        CurrentlySelectedUser.worldCreationProcess.Subscribe(OnWorldCreated);
        CurrentlySelectedUser.worldJoinnedProcess.Subscribe(OnWorldJoinned);
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

    public void RegisterNewUser(string _firstName, string _lastName, string _userName, string _password, User _user)
    {
        Dictionary<string, string> _info = new Dictionary<string, string>();

        // SHA256 _sha256 = SHA256.Create();
        // byte[] _bytes = Encoding.UTF8.GetBytes(_userName + ":" + _password);
        // byte[] _hash = _sha256.ComputeHash(_bytes);
        // StringBuilder _stringBuilder = new StringBuilder();
        // for (int i = 0; i < _hash.Length; i++)
        // {
        //     _stringBuilder.Append(_hash[i].ToString("x2"));
        // }
        // string _hashString = _stringBuilder.ToString();
        // CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg(_hashString, 1f));
        _user.registrationProcess.Subscribe((_) =>
        {
            UpdateSelectedUser(_user);
        });
        _info[Keys.FIRST_NAME] = _firstName;
        _info[Keys.LAST_NAME] = _lastName;
        _info[Keys.USER_NAME] = _userName;
        _info[Keys.PASSWORD] = _password;
        _user.Register(_info);
    }

    public void LogIn(User _user)
    {
        _user.LogIn();
        onLoggedIn?.Invoke(_user.UserId); // TODO:: change during authentication implementation
        UpdateSelectedUser(_user);
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
        CurrentlySelectedUser.CreateAvatar(_avatarName, _viewId);
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
        CurrentlySelectedUser.CreateWorld(_worldName, _viewId);
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
        CurrentlySelectedUser.JoinWorld(m_selectedAvatar.avatarId, m_selectedWorld.worldId);
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
        CurrentlySelectedUser.GetAllMyAvatars();
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
        CurrentlySelectedUser.GetAllWorlds();
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
        CurrentlySelectedUser.GetAllAvatarsFromWorld(_worldId);
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
        CurrentlySelectedUser.SendMessageToReceiver(m_selectedWorld.worldId, _receiverId, _msg);
    }
}

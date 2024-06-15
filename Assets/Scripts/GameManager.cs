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
    private List<AvatarInfo> m_defaultAvatars = new List<AvatarInfo>();
    [SerializeField]
    private List<WorldInfo> m_defaultWorlds = new List<WorldInfo>();

    private User m_selectedUser;
    private List<User> m_users = new List<User>();

    public ReadOnlyCollection<AvatarInfo> DefaultAvatars => new ReadOnlyCollection<AvatarInfo>(m_defaultAvatars);
    public ReadOnlyCollection<WorldInfo> DefaultWorlds => new ReadOnlyCollection<WorldInfo>(m_defaultWorlds);

    private ProcessStatus m_lastLogInStatus;
    private ProcessStatus m_logInStatus;
    private ProcessStatus m_lastAvatarCreationStatus;
    private ProcessStatus m_avatarCreationStatus;
    private ProcessStatus m_lastWorldCreationStatus;
    private ProcessStatus m_worldCreationStatus;
    private ProcessStatus m_lastGetAllWorldsStatus;
    private ProcessStatus m_getAllWorldsStatus;
    private ProcessStatus m_getAllAvatarsStatus;
    private ProcessStatus m_lastGetAllAvatarsStatus;

    public Action onLogInCompleted;
    public Action<AvatarInfo> onAvatarCreationCompleted;
    public Action<WorldInfo> onWorldCreationCompleted;
    public Action<List<AvatarInfo>> onGetAllAvatarsCompleted;
    public Action<List<WorldInfo>> onGetAllWorldsCompleted;

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
        m_lastLogInStatus = ProcessStatus.None;
        m_logInStatus = ProcessStatus.None;

        m_lastAvatarCreationStatus = ProcessStatus.None;
        m_avatarCreationStatus = ProcessStatus.None;

        m_lastWorldCreationStatus = ProcessStatus.None;
        m_worldCreationStatus = ProcessStatus.None;

        m_lastGetAllAvatarsStatus = ProcessStatus.None;
        m_getAllAvatarsStatus = ProcessStatus.None;

        m_lastGetAllWorldsStatus = ProcessStatus.None;
        m_getAllWorldsStatus = ProcessStatus.None;
    }

    private void Update()
    {
        if (m_lastLogInStatus != m_logInStatus)
        {
            onLogInCompleted?.Invoke();
        }

        if (m_lastAvatarCreationStatus != m_avatarCreationStatus)
        {
            onAvatarCreationCompleted?.Invoke();
        }

        if (m_lastWorldCreationStatus != m_worldCreationStatus)
        {
            onWorldCreationCompleted?.Invoke();
        }

        if (m_lastGetAllAvatarsStatus != m_getAllAvatarsStatus)
        {
            onGetAllAvatarsCompleted?.Invoke();
        }

        if (m_lastGetAllWorldsStatus != m_getAllWorldsStatus)
        {
            onGetAllWorldsCompleted?.Invoke();
        }

        m_lastLogInStatus = m_logInStatus;
        m_lastAvatarCreationStatus = m_avatarCreationStatus;
        m_lastWorldCreationStatus = m_worldCreationStatus;
        m_lastGetAllAvatarsStatus = m_getAllAvatarsStatus;
        m_lastGetAllWorldsStatus = m_getAllWorldsStatus;
    }

    public void SignIn()
    {
        User _user = Instantiate(m_userPrefab);
        m_selectedUser = _user;
        m_users.Add(_user);
        m_logInStatus = ProcessStatus.Completed;
    }

    public void SignUp()
    {
        User _user = Instantiate(m_userPrefab);
        m_selectedUser = _user;
        m_users.Add(_user);
        m_logInStatus = ProcessStatus.Completed;
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
        m_avatarCreationStatus = ProcessStatus.Running;
        m_lastAvatarCreationStatus = ProcessStatus.Running;
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
        m_worldCreationStatus = ProcessStatus.Running;
        m_lastWorldCreationStatus = ProcessStatus.Running;
    }

    public void JoinWorld(string _worldId)
    {
        if (m_selectedUser == null)
        {
#if UNITY_EDITOR
            Debug.Log("No User is selected. First SignIn or SignUp.");
#endif
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Joining World" + _worldId + "...");
#endif
    }

    public void GetAllAvatars()
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
        m_selectedUser.GetAllAvatars();
        m_getAllAvatarsStatus = ProcessStatus.Running;
        m_lastGetAllAvatarsStatus = ProcessStatus.Running;
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
        m_getAllWorldsStatus = ProcessStatus.Running;
        m_lastGetAllWorldsStatus = ProcessStatus.Running;
    }

    public void AvatarCreationCompleted()
    {
        m_avatarCreationStatus = ProcessStatus.Completed;
    }

    public void WorldCreationCompleted()
    {
        m_worldCreationStatus = ProcessStatus.Completed;
    }

    public void GetAllAvatarsCompleted()
    {
        m_getAllAvatarsStatus = ProcessStatus.Completed;
    }

    public void GetAllWorldsCompleted()
    {
        m_getAllWorldsStatus = ProcessStatus.Completed;
    }
}

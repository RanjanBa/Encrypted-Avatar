using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;

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
    public Action onLoggedInCompleted;
    public Action onAvatarCreationCompleted;
    public Action onWorldCreationCompleted;

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

    public void SignIn()
    {
        User _user = Instantiate(m_userPrefab);
        m_selectedUser = _user;
        m_users.Add(_user);
        onLoggedInCompleted?.Invoke();
    }

    public void SignUp()
    {
        User _user = Instantiate(m_userPrefab);
        m_selectedUser = _user;
        m_users.Add(_user);
        onLoggedInCompleted?.Invoke();
    }

    public void CreateAvatar(string _avatarName)
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
        m_selectedUser.CreateAvatar(_avatarName);
        onAvatarCreationCompleted?.Invoke();
    }

    public void CreateWorld(string _worldName)
    {
#if UNITY_EDITOR
        Debug.Log("Creating World" + _worldName + "...");
#endif
        onWorldCreationCompleted?.Invoke();
    }

    public void JoinWorld(string _worldId)
    {
#if UNITY_EDITOR
        Debug.Log("Joining World" + _worldId + "...");
#endif
    }
}

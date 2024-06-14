using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get { return m_instance; } }

    private static GameManager m_instance;

    [SerializeField]
    private GameObject m_connectPanel;
    [SerializeField]
    private TMP_InputField m_playerNameInputField;
    [SerializeField]
    private Button m_connectBtn;
    [SerializeField]
    private AvatarUIView m_leftAvatarView;
    [SerializeField]
    private AvatarUIView m_rightAvatarView;

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

    public void CreateAvatar(string _avatarName)
    {
#if UNITY_EDITOR
        Debug.Log("Creating Avatar...");
#endif
    }

    public void CreateWorld(string _worldName)
    {
#if UNITY_EDITOR
        Debug.Log("Creating World...");
#endif
    }

    public void JoinWorld(string _worldId)
    {
#if UNITY_EDITOR
        Debug.Log("Joining World...");
#endif
    }
}

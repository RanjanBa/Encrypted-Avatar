using System.Collections;
using System.Collections.Generic;
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

    public AvatarUIView LeftAvatarView
    {
        get { return m_leftAvatarView; }
    }

    public AvatarUIView RightAvatarView
    {
        get { return m_rightAvatarView; }
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
        m_connectBtn.onClick.AddListener(() =>
        {
            if (m_playerNameInputField == null || m_playerNameInputField.text == "") return;

            Metaverse.Instance.CreateAvatar(m_playerNameInputField.text);
        });

        m_leftAvatarView.SendMsgBtn.onClick.AddListener(() =>
        {
            if (m_rightAvatarView.Avatar == null)
            {
                Debug.Log("No client is there.");
                return;
            }

            string msg = "send_msg\n" + GameManager.Instance.RightAvatarView.Avatar.alias + "\n" + m_leftAvatarView.MessageInputField.text;
            m_leftAvatarView.Avatar.SendMessageToServer(msg);
        });

        m_rightAvatarView.SendMsgBtn.onClick.AddListener(() =>
        {
            if (m_leftAvatarView.Avatar == null)
            {
                Debug.Log("No client is there.");
                return;
            }

            string msg = "send_msg\n" + GameManager.Instance.LeftAvatarView.Avatar.alias + "\n" + m_rightAvatarView.MessageInputField.text;
            m_rightAvatarView.Avatar.SendMessageToServer(msg);
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            m_connectPanel.SetActive(!m_connectPanel.activeSelf);
        }
    }
}

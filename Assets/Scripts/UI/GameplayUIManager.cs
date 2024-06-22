using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject m_worldPanel;
    [SerializeField]
    private GameObject m_avatarSelectionChatPanel;
    [SerializeField]
    private GameObject m_chatViewPanel;
    [SerializeField]
    private Button m_selectAvatarBtn;
    [SerializeField]
    private TMP_Text m_worldNameText;
    [SerializeField]
    private ChatUIView m_leftChatView;
    [SerializeField]
    private ChatUIView m_rightChatView;

    public ChatUIView LeftChatView => m_leftChatView;
    public ChatUIView RightChatView => m_rightChatView;

    private void Start()
    {
        m_selectAvatarBtn.onClick.AddListener(() =>
        {
            m_worldPanel.SetActive(false);
            m_chatViewPanel.SetActive(false);
            m_avatarSelectionChatPanel.SetActive(true);
        });
    }

    private void OnEnable()
    {
        m_worldPanel.SetActive(true);
        m_avatarSelectionChatPanel.SetActive(false);
        m_chatViewPanel.SetActive(false);
        m_leftChatView.UpdateAvatarView(null);
        m_rightChatView.UpdateAvatarView(null);
        GameManager.Instance.onSelectedWorldChanged += OnNewWorldJoinned;
    }

    private void OnDisable()
    {
        GameManager.Instance.onSelectedWorldChanged -= OnNewWorldJoinned;
    }

    private void OnNewWorldJoinned(WorldInfo _worldInfo)
    {
        m_worldNameText.text = _worldInfo.worldName;
    }

    public bool CanUpdateChatView(AvatarInfo _avatarInfo, bool _isLeft)
    {
        if (_isLeft)
        {
            if (m_rightChatView.AvatarInfo != null)
            {
                if (_avatarInfo.avatarId == m_rightChatView.AvatarInfo.avatarId)
                {
#if UNITY_EDITOR
                    Debug.Log("Select Different Avatar for left Chat View...");
#endif
                    return false;
                }
            }
            m_leftChatView.UpdateAvatarView(_avatarInfo);
        }
        else
        {
            if (m_leftChatView.AvatarInfo != null)
            {
                if (_avatarInfo.avatarId == m_leftChatView.AvatarInfo.avatarId)
                {
#if UNITY_EDITOR
                    Debug.Log("Select Different Avatar for right Chat View...");
#endif
                    return false;
                }
            }
            m_rightChatView.UpdateAvatarView(_avatarInfo);
        }

        return true;
    }

    public void AvatarSelectionDone()
    {
        m_worldPanel.SetActive(true);
        m_chatViewPanel.SetActive(true);
        m_avatarSelectionChatPanel.SetActive(false);
    }

    public void SendMsg(ChatUIView _chatView)
    {
        if (string.IsNullOrEmpty(_chatView.GetInputText()))
        {
#if UNITY_EDITOR
            Debug.Log("No message is typed.");
#endif
            return;
        }

        AvatarInfo _receiverInfo = null;
        if (_chatView == m_leftChatView)
        {
            _receiverInfo = m_rightChatView.AvatarInfo;
        }
        else if (_chatView == m_rightChatView)
        {
            _receiverInfo = m_leftChatView.AvatarInfo;
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogWarning("Given ChatViewPanel is not valid.");
        }
#endif

        if (_receiverInfo == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Receiver Avatar id is not found.");
#endif
            return;
        }

        string _msg = _chatView.GetInputText();
        if (string.IsNullOrEmpty(_msg))
        {
#if UNITY_EDITOR
            Debug.LogWarning("No Message is typed in input field.");
#endif
            return;
        }
        GameManager.Instance.SendMessageToReceiver(_receiverInfo.avatarId, _msg);
    }
}

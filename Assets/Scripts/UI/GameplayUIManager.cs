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
}

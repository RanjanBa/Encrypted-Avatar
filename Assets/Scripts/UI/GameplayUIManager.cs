using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject m_mainMenuPanel;
    [SerializeField]
    private GameObject m_avatarSelectionChatPanel;
    [SerializeField]
    private GameObject m_chatViewPanel;
    [SerializeField]
    private GameObject m_worldPanel;
    [SerializeField]
    private Button m_gotoMainMenuBtn;
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
        m_worldPanel.SetActive(true);
        m_avatarSelectionChatPanel.SetActive(false);
        m_chatViewPanel.SetActive(false);
        m_selectAvatarBtn.onClick.AddListener(() =>
        {
            m_worldPanel.SetActive(false);
            m_chatViewPanel.SetActive(false);
            m_avatarSelectionChatPanel.SetActive(true);
        });
        m_gotoMainMenuBtn.onClick.AddListener(() =>
        {
            m_mainMenuPanel.SetActive(true);
            gameObject.SetActive(false);
        });
    }

    private void OnEnable()
    {
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

    public void UpdateChatView(AvatarInfo _avatarInfo, bool _isLeft)
    {
        if (_isLeft)
        {
            m_leftChatView.UpdateAvatarView(_avatarInfo);
        }
        else
        {
            m_rightChatView.UpdateAvatarView(_avatarInfo);
        }
    }

    public void AvatarSelectionDone()
    {
        m_worldPanel.SetActive(true);
        m_chatViewPanel.SetActive(true);
        m_avatarSelectionChatPanel.SetActive(false);
    }
}

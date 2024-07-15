using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;


public class ChatUIView : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_avatarNameText;
    [SerializeField]
    private TMP_Text m_receiveMessageText;
    [SerializeField]
    private TMP_InputField m_messageInputField;
    [SerializeField]
    private Image m_avatarIcon;
    [SerializeField]
    private Button m_sendMsgBtn;
    [SerializeField]
    private GameObject m_receiveMessagePanel;

    private AvatarInfo m_avatarInfo;

    public AvatarInfo AvatarInfo => m_avatarInfo;

    private void OnEnable()
    {
        m_receiveMessagePanel.SetActive(false);
        GameManager.Instance.onMessageReceived += OnMessageReceived;
    }

    private void OnDisable()
    {
        GameManager.Instance.onMessageReceived -= OnMessageReceived;
    }

    private void OnMessageReceived(Dictionary<string, string> _msgInfo)
    {
        if (m_avatarInfo == null)
        {
#if UNITY_EDITOR
            Debug.Log("No avatar is assigned.");
#endif
            return;
        }
        if (_msgInfo[Keys.AVATAR_ID] != m_avatarInfo.avatarId)
        {
#if UNITY_EDITOR
            Debug.Log("Receiver Id and Chat View Avatar Id is not equal.");
#endif
            return;
        }
        m_receiveMessagePanel.SetActive(true);
        m_receiveMessageText.text = _msgInfo[Keys.MESSAGE];
    }

    public void UpdateAvatarView(AvatarInfo _avatarInfo)
    {
        m_avatarInfo = _avatarInfo;
        m_avatarNameText.text = _avatarInfo != null ? _avatarInfo.avatarName : "";
        m_avatarIcon.sprite = _avatarInfo != null ? GameManager.Instance.GetAvatarSprite(_avatarInfo.avatarViewId) : null;
    }

    public string GetInputText()
    {
        return m_messageInputField.text;
    }
}

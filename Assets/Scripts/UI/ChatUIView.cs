using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class ChatUIView : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_avatarNameText;
    [SerializeField]
    private TMP_Text m_receiveMessageText;
    [SerializeField]
    private TMP_InputField m_messageInputField;
    [SerializeField]
    private Button m_sendMsgBtn;
    [SerializeField]
    private GameObject m_receiveMessagePanel;

    private DigitalAvatar m_avatar;

    public DigitalAvatar Avatar
    {
        get { return m_avatar; }
        set
        {
            m_avatar = value;
            UpdateAvatarView(m_avatar);
            // m_avatar.onMessageReceived += UpdateTextView;
        }
    }

    public Button SendMsgBtn
    {
        get { return m_sendMsgBtn; }
    }

    public TMP_InputField MessageInputField
    {
        get { return m_messageInputField; }
    }

    private void UpdateAvatarView(DigitalAvatar _avatar)
    {
        // m_avatarNameText.text = _avatar.AvatarName;
    }

    private void UpdateTextView(string _msg)
    {
        m_receiveMessageText.text = _msg;
    }
}

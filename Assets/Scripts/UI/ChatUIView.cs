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

    private AvatarInfo m_avatarInfo;

    public AvatarInfo AvatarInfo => m_avatarInfo;

    public void UpdateAvatarView(AvatarInfo _avatarInfo)
    {
        m_avatarInfo = _avatarInfo;
        m_avatarNameText.text = _avatarInfo.avatarName;
    }
}

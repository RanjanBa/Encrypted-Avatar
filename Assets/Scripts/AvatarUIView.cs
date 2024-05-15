using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class AvatarUIView : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_avatarNameText;
    [SerializeField]
    private TMP_Text m_messageText;
    [SerializeField]
    private TMP_InputField m_messageInputField;
    [SerializeField]
    private Button m_sendMsgBtn;

    private Client m_avatar;

    public Client Avatar
    {
        get { return m_avatar; }
        set { 
            m_avatar = value;
            UpdateAvatarView(m_avatar);
            m_avatar.onDecryptedMsgReceived += UpdateTextView;
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

    private void UpdateAvatarView(Client _avatar)
    {
        m_avatarNameText.text = _avatar.alias;
    }

    private void UpdateTextView(string _msg)
    {
        m_messageText.text = _msg;
    }
}

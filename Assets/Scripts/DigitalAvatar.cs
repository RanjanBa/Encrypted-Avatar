using UnityEngine;
using UnityEngine.Events;

public class DigitalAvatar : MonoBehaviour
{
    [SerializeField]
    private string m_avatarName;

    public string AvatarName => m_avatarName;

    public UnityAction<string> onMessageReceived;

    public void UpdateAvatarName(string _name)
    {
        m_avatarName = _name;
    }
}
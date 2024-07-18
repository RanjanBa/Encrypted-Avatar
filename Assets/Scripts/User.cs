using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class User : MonoBehaviour
{
    private UserHandler m_userHandler;

    public UserHandler UserHandler => m_userHandler;
    public string UserId => m_userHandler.UserId;

    private void Update()
    {
        m_userHandler?.Update();
    }

    public void AssignUserHandler(UserHandler _userHandler)
    {
        if (_userHandler == null) return;
        m_userHandler = _userHandler;
        m_userHandler.messageReceivedCallback.onSuccessCallbackDuringUpdateFrame += GameManager.Instance.OnMessageReceived;
    }

    private void OnDestroy()
    {
        m_userHandler?.Disconnect();
    }
}

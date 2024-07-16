using System;
using UnityEngine;

public class CallbackHandler<T> where T : class
{
    private CallbackStatus m_lastStatus;
    private CallbackStatus m_status;
    private T m_info;
    private string m_error;

    public Action<T> onSuccessImmediateCallback;
    public Action<T> onSuccessCallbackDuringUpdateFrame;
    public Action<string> onFailureImmediateCallback;
    public Action<string> onFailureCallbackDuringUpdateFrame;

    // public CallbackStatus StatusOfCallback => m_status;

    public CallbackHandler()
    {
        m_lastStatus = CallbackStatus.None;
        m_status = CallbackStatus.None;
        m_info = null;
    }

    public void ChangeToNone()
    {
        m_lastStatus = CallbackStatus.None;
        m_status = CallbackStatus.None;
        m_info = null;
        m_error = null;
    }

    public void ChangeToPending()
    {
        m_lastStatus = m_status;
        m_status = CallbackStatus.Pending;
    }

    public void ChangeToSuccess(T _info)
    {
        m_lastStatus = m_status;
        m_status = CallbackStatus.Success;
        m_info = _info;
        onSuccessImmediateCallback?.Invoke(m_info);
    }

    public void ChangeToFailure(string _error)
    {
        m_lastStatus = m_status;
        m_status = CallbackStatus.Failure;
        m_error = _error;
        onFailureImmediateCallback?.Invoke(_error);
    }

    public void Update()
    {
        if (m_lastStatus != m_status)
        {
            if (m_status == CallbackStatus.Success)
            {
#if UNITY_EDITOR
                if (onSuccessCallbackDuringUpdateFrame == null)
                {
                    Debug.LogWarning("No method is subscribed to update frame success callback");
                }
#endif
                onSuccessCallbackDuringUpdateFrame?.Invoke(m_info);
            }
            else if (m_status == CallbackStatus.Failure)
            {
#if UNITY_EDITOR
                if (onFailureCallbackDuringUpdateFrame == null)
                {
                    Debug.LogWarning("No method is subscribed to update frame failure callback");
                }
#endif
                onFailureCallbackDuringUpdateFrame?.Invoke(m_error);
            }
        }

        m_lastStatus = m_status;
    }
}

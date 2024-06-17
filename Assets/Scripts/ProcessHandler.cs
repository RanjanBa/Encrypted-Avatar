using System;
using System.Reflection;
using UnityEngine;

public class ProcessHandler<T> where T : class
{
    private ProcessStatus m_lastStatus;
    private ProcessStatus m_status;
    private T m_info;

    public Action<T> onProcessCompleted;

    public ProcessHandler()
    {
        m_lastStatus = ProcessStatus.None;
        m_status = ProcessStatus.None;
        m_info = null;
    }

    public void ChangeProcessToNone()
    {
        m_status = ProcessStatus.None;
        m_info = null;
    }

    public void ChangeProcessToRunning()
    {
        m_status = ProcessStatus.Running;
    }

    public void ChangeProcessToCompleted(T _info)
    {
        m_status = ProcessStatus.Completed;
        m_info = _info;
    }

    public void UpdateProcess()
    {
        if (m_lastStatus != m_status)
        {
            if (m_status == ProcessStatus.Completed)
            {
#if UNITY_EDITOR
                if (onProcessCompleted != null)
                {
                    Debug.LogWarning("No method is subscribed to process");
                }
#endif
                onProcessCompleted?.Invoke(m_info);
            }
        }

        m_lastStatus = m_status;
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance { get { return m_instance; } }

    private static CanvasManager m_instance;

    [SerializeField]
    private GameObject m_authenticationPanel;
    [SerializeField]
    private GameObject m_mainMenuPanel;
    [SerializeField]
    private GameObject m_gameplayPanel;
    [SerializeField]
    private TMP_Text m_userIdText;
    [SerializeField]
    private GameObject m_errorMsgPanel;
    [SerializeField]
    private TMP_Text m_errorMsgText;

    private GameObject m_currentActivePanel;
    private Stack<GameObject> m_lastActivePanels;

    public Queue<ErrorMsg> errorMsg;

    private void Awake()
    {
        if (m_instance != null)
        {
            if (m_instance.gameObject == gameObject)
            {
                Destroy(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        m_instance = this;
        errorMsg = new Queue<ErrorMsg>();
    }

    private void Start()
    {
        m_authenticationPanel.SetActive(true);
        m_mainMenuPanel.SetActive(false);
        m_gameplayPanel.SetActive(false);
        m_lastActivePanels = new Stack<GameObject>();
        m_currentActivePanel = m_authenticationPanel;

        GameManager.Instance.onLoggedIn += (_) =>
        {
            ActivatePanel(m_mainMenuPanel);
        };

        GameManager.Instance.onAvatarCreated += (_) =>
        {
            ActivatePanel(m_mainMenuPanel);
        };

        GameManager.Instance.onWorldCreated += (_) =>
        {
            ActivatePanel(m_mainMenuPanel);
        };

        GameManager.Instance.onWorldJoinned += (_) =>
        {
            ActivatePanel(m_gameplayPanel);
        };

        GameManager.Instance.onSelectedUserChanged += (_user) =>
        {
            m_userIdText.text = _user.UserId;
        };
    }

    private void Update()
    {
        if (errorMsg != null && errorMsg.Count > 0)
        {
            m_errorMsgPanel.SetActive(true);
            ErrorMsg _error = errorMsg.Peek();
            if (_error.duration > 0)
            {
                _error.duration -= Time.deltaTime;
                m_errorMsgText.text = _error.msg;
            }
            else
            {
                errorMsg.Dequeue();
            }
        }
        else
        {
            m_errorMsgPanel.SetActive(false);
        }
    }

    public void OnBack()
    {
        if (m_lastActivePanels.Count == 0) return;

        m_currentActivePanel.SetActive(false);
        GameObject _gm = m_lastActivePanels.Pop();
        _gm.SetActive(true);
        m_currentActivePanel = _gm;

        if (m_currentActivePanel == m_mainMenuPanel)
        {
            m_lastActivePanels.Clear();
        }
    }

    public void ActivatePanel(GameObject _panel)
    {
        m_currentActivePanel.SetActive(false);
        if (_panel == null)
        {
            m_lastActivePanels.Clear();
            m_currentActivePanel = null;
            return;
        }

        if (_panel == m_mainMenuPanel || _panel == m_gameplayPanel)
        {
            m_lastActivePanels.Clear();
        }
        else
        {
            m_lastActivePanels.Push(m_currentActivePanel);
        }

        _panel.SetActive(true);
        m_currentActivePanel = _panel;
    }
}

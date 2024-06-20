using System.Collections.Generic;
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

    private GameObject m_currentActivePanel;
    private Stack<GameObject> m_lastActivePanels;

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
    }

    private void Start()
    {
        m_authenticationPanel.SetActive(true);
        m_mainMenuPanel.SetActive(false);
        m_gameplayPanel.SetActive(false);
        m_lastActivePanels = new Stack<GameObject>();
        m_currentActivePanel = m_authenticationPanel;

        GameManager.Instance.logInProcess.Subscribe((_) =>
        {
            ActivatePanel(m_mainMenuPanel);
        });

        GameManager.Instance.avatarCreationProcess.Subscribe((_) =>
        {
            ActivatePanel(m_mainMenuPanel);
        });

        GameManager.Instance.worldCreationProcess.Subscribe((_) =>
        {
            ActivatePanel(m_mainMenuPanel);
        });

        GameManager.Instance.worldJoinnedProcess.Subscribe((_) =>
        {
            ActivatePanel(m_gameplayPanel);
        });
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

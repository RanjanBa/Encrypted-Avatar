using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public enum JoinOrCreateDecision
    {
        None = 0,
        CreateWorld,
        JoinWorld
    }

    [SerializeField]
    private GameObject m_menuPanel;
    [SerializeField]
    private GameObject m_avatarSelectionPanel;
    [SerializeField]
    private GameObject m_avatarCreationPanel;
    [SerializeField]
    private GameObject m_worldSelectionPanel;
    [SerializeField]
    private GameObject m_worldCreationPanel;
    [SerializeField]
    private GameObject m_joinWorldPanel;
    [SerializeField]
    private GameObject m_decisionPanel;

    [Header("Menu Buttons")]
    [SerializeField]
    private Button m_createAvatarMenuBtn;
    [SerializeField]
    private Button m_createWorldMenuBtn;
    [SerializeField]
    private Button m_joinWorldMenuBtn;

    [Header("Other Buttons")]
    [SerializeField]
    private Button m_createAvatarBtn;
    [SerializeField]
    private Button m_createWorldBtn;
    [SerializeField]
    private Button m_joinWorldBtn;
    [SerializeField]
    private Button m_decisionContinueBtn;

    private GameObject m_currentActivePanel;
    private Stack<GameObject> m_lastActivePanels;
    private JoinOrCreateDecision m_selectedDecision;

    private void Start()
    {
        m_menuPanel.SetActive(true);
        m_avatarSelectionPanel.SetActive(false);
        m_avatarCreationPanel.SetActive(false);
        m_worldSelectionPanel.SetActive(false);
        m_worldCreationPanel.SetActive(false);
        m_joinWorldPanel.SetActive(false);
        m_decisionPanel.SetActive(false);
        m_lastActivePanels = new Stack<GameObject>();
        m_currentActivePanel = m_menuPanel;

        m_createAvatarMenuBtn.onClick.AddListener(() =>
        {
            m_lastActivePanels.Push(m_currentActivePanel);
            m_currentActivePanel.SetActive(false);
            m_avatarSelectionPanel.SetActive(true);
            m_currentActivePanel = m_avatarSelectionPanel;
        });

        m_createWorldMenuBtn.onClick.AddListener(() =>
        {
            m_lastActivePanels.Push(m_currentActivePanel);
            m_currentActivePanel.SetActive(false);
            m_worldSelectionPanel.SetActive(true);
            m_currentActivePanel = m_worldSelectionPanel;
        });

        m_joinWorldMenuBtn.onClick.AddListener(() =>
        {
            m_lastActivePanels.Push(m_currentActivePanel);
            m_currentActivePanel.SetActive(false);
            m_joinWorldPanel.SetActive(true);
            m_currentActivePanel = m_joinWorldPanel;
        });

        m_createAvatarBtn.onClick.AddListener(() =>
        {
            m_lastActivePanels.Push(m_currentActivePanel);
            m_currentActivePanel.SetActive(false);
            m_decisionPanel.SetActive(true);
            m_currentActivePanel = m_decisionPanel;

            GameManager.Instance.CreateAvatar("Test");
        });

        m_createWorldBtn.onClick.AddListener(() =>
        {
            m_lastActivePanels.Push(m_currentActivePanel);
            m_currentActivePanel.SetActive(false);
            m_menuPanel.SetActive(true);
            m_currentActivePanel = m_menuPanel;

            GameManager.Instance.CreateWorld("Meeting A");
        });

        m_joinWorldBtn.onClick.AddListener(() =>
        {
            m_currentActivePanel.SetActive(false);
            m_currentActivePanel = null;
            m_lastActivePanels.Clear();

            GameManager.Instance.JoinWorld("12345");
        });

        m_decisionContinueBtn.onClick.AddListener(() =>
        {
            if (m_selectedDecision == JoinOrCreateDecision.None)
            {
#if UNITY_EDITOR
                Debug.Log("Select Your Decision...");
#endif
                return;
            }

            if (m_selectedDecision == JoinOrCreateDecision.CreateWorld)
            {
                m_lastActivePanels.Push(m_currentActivePanel);
                m_currentActivePanel.SetActive(false);
                m_worldSelectionPanel.SetActive(true);
                m_currentActivePanel = m_worldSelectionPanel;
            }
            else if (m_selectedDecision == JoinOrCreateDecision.JoinWorld)
            {
                m_lastActivePanels.Push(m_currentActivePanel);
                m_currentActivePanel.SetActive(false);
                m_joinWorldPanel.SetActive(true);
                m_currentActivePanel = m_joinWorldPanel;
            }
        });

        m_selectedDecision = JoinOrCreateDecision.None;
    }

    public void OnBack()
    {
        m_currentActivePanel.SetActive(false);
        GameObject _gm = m_lastActivePanels.Pop();
        _gm.SetActive(true);
        m_currentActivePanel = _gm;

        if (m_currentActivePanel == m_menuPanel)
        {
            m_lastActivePanels.Clear();
        }
    }

    public void OnContinue(GameObject _panel)
    {
        m_lastActivePanels.Push(m_currentActivePanel);
        m_currentActivePanel.SetActive(false);
        _panel.SetActive(true);
        m_currentActivePanel = _panel;
    }

    public void OnJoinOrCreateSelect(string _decision)
    {
        if (string.Equals(_decision, "world"))
        {
            m_selectedDecision = JoinOrCreateDecision.CreateWorld;
        }
        else if (string.Equals(_decision, "join"))
        {
            m_selectedDecision = JoinOrCreateDecision.JoinWorld;
        }
        else
        {
            m_selectedDecision = JoinOrCreateDecision.None;
        }
    }
}

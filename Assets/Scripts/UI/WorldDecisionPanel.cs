using UnityEngine;
using UnityEngine.UI;

public class WorldDecisionPanel : MonoBehaviour
{
    [SerializeField]
    private Button m_worldCreationBtn;
    [SerializeField]
    private Button m_worldJoinBtn;
    [SerializeField]
    private Button m_mainMenuBtn;
    [SerializeField]
    private GameObject m_worldCreationPanel;
    [SerializeField]
    private GameObject m_worldJoinPanel;
    [SerializeField]
    private GameObject m_mainMenuPanel;

    private void Start()
    {
        m_worldCreationBtn.onClick.AddListener(() =>
        {
            CanvasManager.Instance.ActivatePanel(m_worldCreationPanel);
        });
        m_worldJoinBtn.onClick.AddListener(() =>
        {
            CanvasManager.Instance.ActivatePanel(m_worldJoinPanel);
        });
        m_mainMenuBtn.onClick.AddListener(() =>
        {
            CanvasManager.Instance.ActivatePanel(m_mainMenuPanel);
        });
    }
}

using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField]
    private Button m_createAvatarBtn;
    [SerializeField]
    private Button m_createWorldBtn;
    [SerializeField]
    private Button m_joinWorldBtn;
    [SerializeField]
    private Button m_selectWorldBtn;
    [SerializeField]
    private GameObject m_avatarCreationPanel;
    [SerializeField]
    private GameObject m_worldCreationPanel;
    [SerializeField]
    private GameObject m_joinWorldPanel;
    [SerializeField]
    private GameObject m_selectWorldPanel;

    private void Start()
    {
        m_createAvatarBtn.onClick.AddListener(() =>
        {
            CanvasManager.Instance.ActivatePanel(m_avatarCreationPanel);
        });
        m_createWorldBtn.onClick.AddListener(() =>
        {
            CanvasManager.Instance.ActivatePanel(m_worldCreationPanel);
        });
        m_joinWorldBtn.onClick.AddListener(() =>
        {
            CanvasManager.Instance.ActivatePanel(m_joinWorldPanel);
        });
        m_selectWorldBtn.onClick.AddListener(() =>
        {
            CanvasManager.Instance.ActivatePanel(m_selectWorldPanel);
        });
    }
}

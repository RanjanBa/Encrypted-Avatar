using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField]
    private Button m_createAvatarBtn;
    [SerializeField]
    private Button m_selectAvatarBtn;
    [SerializeField]
    private GameObject m_avatarCreationPanel;
    [SerializeField]
    private GameObject m_avatarSelectionPanel;

    private void Start()
    {
        m_createAvatarBtn.onClick.AddListener(() =>
        {
            CanvasManager.Instance.ActivatePanel(m_avatarCreationPanel);
        });
        m_selectAvatarBtn.onClick.AddListener(() =>
        {
            CanvasManager.Instance.ActivatePanel(m_avatarSelectionPanel);
        });
    }
}

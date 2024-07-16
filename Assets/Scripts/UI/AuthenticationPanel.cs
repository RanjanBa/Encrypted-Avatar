using UnityEngine;
using UnityEngine.UI;

public class AuthenticationPanel : MonoBehaviour
{
    [SerializeField]
    private Button m_logInBtn;
    [SerializeField]
    private Button m_registerBtn;
    [SerializeField]
    private GameObject m_loginPanel;
    [SerializeField]
    private GameObject m_registerPanel;

    private void Start()
    {
        m_logInBtn.onClick.AddListener(() =>
        {
            CanvasManager.Instance.ActivatePanel(m_loginPanel);
        });
        m_registerBtn.onClick.AddListener(() =>
        {
            CanvasManager.Instance.ActivatePanel(m_registerPanel);
        });
    }
}

using UnityEngine;
using UnityEngine.UI;

public class AuthenticationPanel : MonoBehaviour
{
    [SerializeField]
    private Button m_logInBtn;
    [SerializeField]
    private Button m_registerBtn;
    [SerializeField]
    private GameObject m_registerPanel;

    private void Start()
    {
        m_logInBtn.onClick.AddListener(() =>
        {
            // GameManager.Instance.LogIn();
        });
        m_registerBtn.onClick.AddListener(() =>
        {
            // GameManager.Instance.RegisterNewUser();
            CanvasManager.Instance.ActivatePanel(m_registerPanel);
        });
    }
}

using UnityEngine;
using UnityEngine.UI;

public class AuthenticationPanel : MonoBehaviour
{
    [SerializeField]
    private Button m_logInBtn;
    [SerializeField]
    private Button m_registerBtn;

    private void Start()
    {
        m_logInBtn.onClick.AddListener(() =>
        {
        });
        m_registerBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.RegisterUp();
        });
    }
}

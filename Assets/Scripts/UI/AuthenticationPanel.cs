using UnityEngine;
using UnityEngine.UI;

public class AuthenticationPanel : MonoBehaviour
{
    [SerializeField]
    private Button m_signInBtn;
    [SerializeField]
    private Button m_signUpBtn;

    private void Start()
    {
        m_signInBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.SignIn();
        });
        m_signUpBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.SignUp();
        });
    }
}

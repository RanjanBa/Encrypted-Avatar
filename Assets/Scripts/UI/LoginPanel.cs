using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField m_userNameInputField;
    [SerializeField]
    private TMP_InputField m_passwordInputField;
    [SerializeField]
    private Button m_loginBtn;
    [SerializeField]
    private Button m_backBtn;
    [SerializeField]
    private User m_userPrefab;

    private void Start()
    {
        m_loginBtn.interactable = false;
        m_loginBtn.onClick.AddListener(() =>
        {
            if (string.IsNullOrEmpty(m_userNameInputField.text))
            {
                CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg("Enter username...", 1f));
                return;
            }

            if (string.IsNullOrEmpty(m_passwordInputField.text))
            {
                CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg("Enter password...", 1f));
                return;
            }

            // GameManager.Instance.LogIn(m_userNameInputField.text, m_passwordInputField.text, m_currentUser);
        });
    }

    private void Update()
    {
        if (string.IsNullOrEmpty(m_userNameInputField.text) && string.IsNullOrEmpty(m_passwordInputField.text))
        {
            m_loginBtn.interactable = true;
        }
        else
        {
            m_loginBtn.interactable = false;
        }
    }

    private void OnEnable()
    {
        m_loginBtn.interactable = false;
        m_backBtn.interactable = false;
        // m_currentUser = Instantiate(m_userPrefab);
        // m_currentUser.gameObject.name = "user";
        // m_currentUser.logInProcess.Subscribe(OnUserLoggedIn);
    }

    private void OnDisable()
    {
        // if (m_currentUser != null && !m_currentUser.IsLoggedIn)
        // {
        //     m_currentUser.logInProcess.Unsubscribe(OnUserLoggedIn);
        //     Destroy(m_currentUser.gameObject);
        // }
    }

    private void OnUserLoggedIn(string _userId)
    {
        // m_currentUser.transform.SetParent(m_usersContainer);
        // m_currentUser = null;
        // CanvasManager.Instance.ActivatePanel(m_mainMenuPanel);
    }
}

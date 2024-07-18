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

    private bool m_isLogging;
    private UserHandler m_userHandler;

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

            m_isLogging = true;
            GameManager.Instance.LogIn(m_userNameInputField.text, m_passwordInputField.text, m_userHandler);
        });
    }

    private void Update()
    {
        if (m_userHandler == null || !m_userHandler.IsConnected || string.IsNullOrEmpty(m_userNameInputField.text) || string.IsNullOrEmpty(m_passwordInputField.text) || m_isLogging)
        {
            m_loginBtn.interactable = false;
        }
        else
        {
            m_loginBtn.interactable = true;
        }

        if (m_isLogging)
        {
            m_backBtn.interactable = false;
        }
        else
        {
            m_backBtn.interactable = true;
        }

        m_userHandler?.Update();
    }

    private void OnEnable()
    {
        m_isLogging = false;
        m_userHandler = new UserHandler();
        m_userHandler.logInCallback.onSuccessCallbackDuringUpdateFrame += OnUserLoggedIn;
        m_userHandler.logInCallback.onFailureCallbackDuringUpdateFrame += (error) =>
        {
            m_isLogging = false;
        };
        GameManager.Instance.ConnectUserHandlerWithServer(m_userHandler);
    }

    private void OnDisable()
    {
        if (m_userHandler != null && !m_userHandler.IsLoggedIn)
        {
            m_userHandler.Disconnect();
        }
        m_userHandler = null;
    }

    private void OnUserLoggedIn(string _userId)
    {
        User _user = GameManager.Instance.CreateLoggedInUser(m_userHandler);
        GameManager.Instance.UpdateSelectedUser(_user);
        m_isLogging = false;
        GameManager.Instance.onUserLoggedIn?.Invoke(_userId);
    }
}

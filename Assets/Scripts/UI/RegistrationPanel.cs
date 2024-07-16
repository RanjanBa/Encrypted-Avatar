using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RegistrationPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField m_firstNameInputField;
    [SerializeField]
    private TMP_InputField m_lastNameInputField;
    [SerializeField]
    private TMP_InputField m_userNameInputField;
    [SerializeField]
    private TMP_InputField m_passwordInputField;
    [SerializeField]
    private TMP_InputField m_confirmPasswordInputField;
    [SerializeField]
    private Button m_registerBtn;
    [SerializeField]
    private Button m_backBtn;
    [SerializeField]
    private GameObject m_mainMenuPanel;

    private UserHandler m_userHandler;


    private void Start()
    {
        m_registerBtn.onClick.AddListener(() =>
        {
            if (m_firstNameInputField.text.Length < 5)
            {
                CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg(string.Format("First Name is too short. The length is {0}", m_firstNameInputField.text.Length), 1f));
                return;
            }

            if (m_lastNameInputField.text.Length < 5)
            {
                CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg(string.Format("Last Name is too short. The length is {0}", m_lastNameInputField.text.Length), 1f));
                return;
            }

            if (m_userNameInputField.text.Length < 5)
            {
                CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg(string.Format("User Name is too short. The length is {0}", m_userNameInputField.text.Length), 1f));
                return;
            }

            if (m_passwordInputField.text != m_confirmPasswordInputField.text)
            {
                CanvasManager.Instance.toastMessagesQueue.Enqueue(new ToastMsg("Password is not same...", 1f));
                m_passwordInputField.text = m_confirmPasswordInputField.text = "";
                return;
            }

            GameManager.Instance.RegisterNewUser(m_firstNameInputField.text, m_lastNameInputField.text, m_userNameInputField.text, m_passwordInputField.text, m_userHandler);
            m_registerBtn.interactable = false;
            m_backBtn.interactable = false;
        });
    }

    private void OnEnable()
    {
        m_registerBtn.interactable = true;
        m_backBtn.interactable = true;
        m_userHandler = new UserHandler();
        m_userHandler.registrationCallback.onSuccessCallbackDuringUpdateFrame += OnUserRegistered;
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

    private void OnUserRegistered(string _userId)
    {
        GameManager.Instance.AddRegisteredUser(m_userHandler);
        CanvasManager.Instance.ActivatePanel(m_mainMenuPanel);
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RegistrationPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField m_userNameInputField;
    [SerializeField]
    private TMP_InputField m_passwordInputField;
    [SerializeField]
    private TMP_InputField m_confirmPasswordInputField;
    [SerializeField]
    private Button m_registerBtn;
    [SerializeField]
    private User m_userPrefab;

    private User m_currentUser;

    private void Start()
    {
        m_registerBtn.onClick.AddListener(() =>
        {
            if (m_userNameInputField.text.Length < 5)
            {
                CanvasManager.Instance.errorMessagesQueue.Enqueue(new ErrorMsg(string.Format("Username is too short. The length is {0}", m_userNameInputField.text.Length), 1f));
                return;
            }

            if (m_passwordInputField.text != m_confirmPasswordInputField.text)
            {
                CanvasManager.Instance.errorMessagesQueue.Enqueue(new ErrorMsg("Password is not same...", 1f));
                m_passwordInputField.text = m_confirmPasswordInputField.text = "";
                return;
            }

            GameManager.Instance.RegisterNewUser(m_userNameInputField.text, m_passwordInputField.text, m_currentUser);
            m_registerBtn.interactable = false;
        });
    }

    private void OnEnable()
    {
        m_currentUser = Instantiate(m_userPrefab);
        m_currentUser.gameObject.name = "user " + (GameManager.Instance.Users.Count + 1).ToString();
    }
}

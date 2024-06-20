using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UsersSelectionPanel : MonoBehaviour
{
    [SerializeField, Tooltip("User button Prefab")]
    private Button m_contentPrefab;
    [SerializeField]
    private Transform m_contentContainer;

    private void OnEnable()
    {
        DestroyContents();
        foreach (var _user in GameManager.Instance.Users)
        {
            Button _btn = InstantiateBtn(_user.gameObject.name);
            _btn.onClick.AddListener(() =>
            {
                GameManager.Instance.SignIn(_user);
            });
        }
    }

    private Button InstantiateBtn(string _name)
    {
        Button _btn = Instantiate(m_contentPrefab, m_contentContainer);
        TMP_Text _text = _btn.gameObject.GetComponentInChildren<TMP_Text>();
        if (_text)
        {
            _text.text = _name;
        }

        return _btn;
    }

    private void DestroyContents()
    {
        for (int i = m_contentContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(m_contentContainer.GetChild(i).gameObject);
        }
    }
}

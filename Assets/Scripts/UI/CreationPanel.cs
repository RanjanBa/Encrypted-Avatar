using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreationPanel : MonoBehaviour
{
    private enum CreationType
    {
        Avatar,
        World
    }

    [SerializeField]
    private Button m_createBtn;
    [SerializeField]
    private TMP_InputField m_inputField;
    [SerializeField, Tooltip("Avatar UI or World UI Prefab")]
    private Button m_contentPrefab;
    [SerializeField]
    private Transform m_contentContainer;
    [SerializeField]
    private CardView m_selectedCardView;
    [SerializeField]
    private CreationType m_creationType;
    private string m_viewId;

    private void Start()
    {
        m_selectedCardView.gameObject.SetActive(false);
        m_createBtn.onClick.AddListener(() =>
        {
            if (m_inputField.text.Length < 5)
            {
#if UNITY_EDITOR
                Debug.Log("Input Field length is less than 5...");
#endif
                return;
            }

            if (string.IsNullOrEmpty(m_viewId))
            {
#if UNITY_EDITOR
                if (m_creationType == CreationType.Avatar)
                {
                    Debug.Log("You have to select avatar...");
                }
                else
                {
                    Debug.Log("You have to select world...");
                }
#endif
                return;
            }

            if (m_creationType == CreationType.Avatar)
            {
                GameManager.Instance.CreateAvatar(m_inputField.text, m_viewId);
            }
            else
            {
                GameManager.Instance.CreateWorld(m_inputField.text, m_viewId);
            }
        });
    }

    private void OnEnable()
    {
        m_viewId = null;
        m_selectedCardView.gameObject.SetActive(false);
        for (int i = m_contentContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(m_contentContainer.GetChild(i).gameObject);
        }

        if (m_creationType == CreationType.Avatar)
        {
            ReadOnlyCollection<IconWithID> _avatarInfos = GameManager.Instance.DefaultAvatars;

            foreach (var _info in _avatarInfos)
            {
                InstantiateBtn(null, _info.viewId);
            }
        }
        else
        {
            ReadOnlyCollection<IconWithID> _worldInfos = GameManager.Instance.DefaultWorlds;

            foreach (var _info in _worldInfos)
            {
                InstantiateBtn(null, _info.viewId);
            }
        }
    }

    private void InstantiateBtn(string _name, string _viewId)
    {
        Button _btn = Instantiate(m_contentPrefab, m_contentContainer);
        CardView _cardView = _btn.GetComponent<CardView>();
        Sprite _icon = null;
        if (_cardView)
        {
            if (m_creationType == CreationType.Avatar)
            {
                _icon = GameManager.Instance.GetAvatarSprite(_viewId);
            }
            else
            {
                _icon = GameManager.Instance.GetWorldSprite(_viewId);
            }
            _cardView.UpdateIcon(_icon);
            _cardView.UpdateName(_name);
        }

        _btn.onClick.AddListener(() =>
        {
            m_selectedCardView.gameObject.SetActive(true);
            m_selectedCardView.UpdateIcon(_icon);
            m_selectedCardView.UpdateName(_name);
            m_viewId = _viewId;
#if UNITY_EDITOR
            Debug.Log("Selected avatar view id -> " + _viewId + "...");
#endif
        });
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarSelectionPanel : MonoBehaviour
{
    [SerializeField]
    private GameplayUIManager m_gameplayUIManager;
    [SerializeField]
    private Button m_contentPrefab;
    [SerializeField]
    private Transform m_contentContainer;
    [SerializeField]
    private Button m_leftAvatarBtn;
    [SerializeField]
    private Button m_rightAvatarBtn;
    [SerializeField]
    private CardView m_leftCardView;
    [SerializeField]
    private CardView m_rightCardView;
    [SerializeField]
    private Button m_doneBtn;

    private CardView m_selectedCardView;
    private bool m_isLeftSelected;

    private void Start()
    {
        m_selectedCardView = null;
        m_leftAvatarBtn.onClick.AddListener(() =>
        {
            m_selectedCardView = m_leftCardView;
            m_isLeftSelected = true;
        });
        m_rightAvatarBtn.onClick.AddListener(() =>
        {
            m_selectedCardView = m_rightCardView;
            m_isLeftSelected = false;
        });
        m_doneBtn.onClick.AddListener(() =>
        {
            m_gameplayUIManager.AvatarSelectionDone();
        });
    }

    private void OnEnable()
    {
        DestroyContents();
        GameManager.Instance.getAllAvatarsProcess.Subscribe(OnAvatarsRetrieved);
        StartCoroutine(GetAvatars());
    }

    private IEnumerator GetAvatars()
    {
        yield return new WaitForSeconds(2);
        GameManager.Instance.GetAllAvatarsFromSelectedWorld();
    }

    private void OnDisable()
    {
        GameManager.Instance.getAllAvatarsProcess.Unsubscribe(OnAvatarsRetrieved);
    }

    private void OnAvatarsRetrieved(List<AvatarInfo> _avatars)
    {
#if UNITY_EDITOR
        Debug.Log("Avatars are retrieved from server..");
#endif

        foreach (var _avatar in _avatars)
        {
            Button _btn = InstantiateBtn(_avatar.avatarName, _avatar.avatarViewId);
            _btn.onClick.AddListener(() =>
            {
                if (m_selectedCardView != null)
                {
                    m_selectedCardView.UpdateName(_avatar.avatarName);
                    m_selectedCardView.UpdateIcon(GameManager.Instance.GetAvatarSprite(_avatar.avatarViewId));
                    m_gameplayUIManager.UpdateChatView(_avatar, m_isLeftSelected);
                }
#if UNITY_EDITOR
                else
                {
                    Debug.Log("You have to select left or right card view...");
                }
#endif
            });
        }
    }

    private Button InstantiateBtn(string _name, string _viewId)
    {
        Button _btn = Instantiate(m_contentPrefab, m_contentContainer);
        CardView _cardView = _btn.GetComponent<CardView>();
        if (_cardView)
        {
            Sprite _icon = GameManager.Instance.GetAvatarSprite(_viewId);
            _cardView.UpdateIcon(_icon);
            _cardView.UpdateName(_name);
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

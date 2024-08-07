using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private Toggle m_leftToggle;
    [SerializeField]
    private Toggle m_rightToggle;
    [SerializeField]
    private CardView m_leftCardView;
    [SerializeField]
    private CardView m_rightCardView;
    [SerializeField]
    private Button m_doneBtn;

    private List<Toggle> m_toggles;

    private CardView m_selectedCardView;
    private bool m_isLeftSelected;

    private void Start()
    {
        m_toggles = GetComponentsInChildren<Toggle>().ToList();
        m_selectedCardView = null;
        m_leftToggle.onValueChanged.AddListener((_state) =>
        {
            if (_state)
            {
                m_selectedCardView = m_leftCardView;
                m_isLeftSelected = true;
            }
        });
        m_rightToggle.onValueChanged.AddListener((_state) =>
        {
            if (_state)
            {
                m_selectedCardView = m_rightCardView;
                m_isLeftSelected = false;
            }
        });
        m_doneBtn.onClick.AddListener(() =>
        {
            m_gameplayUIManager.AvatarSelectionDone();
        });
    }

    private void OnEnable()
    {
        DestroyContents();
        if (m_toggles != null)
        {
            foreach (var _toggle in m_toggles)
            {
                _toggle.isOn = false;
            }
        }

        if (m_gameplayUIManager.LeftChatView.AvatarInfo == null)
        {
            m_leftCardView.UpdateName("");
            m_leftCardView.UpdateIcon(null);
        }
        else
        {
            m_leftCardView.UpdateName(m_gameplayUIManager.LeftChatView.AvatarInfo.avatarName);
            m_leftCardView.UpdateIcon(GameManager.Instance.GetAvatarSprite(m_gameplayUIManager.LeftChatView.AvatarInfo.avatarViewId));
        }
        if (m_gameplayUIManager.RightChatView.AvatarInfo == null)
        {
            m_rightCardView.UpdateName("");
            m_rightCardView.UpdateIcon(null);
        }
        else
        {
            m_rightCardView.UpdateName(m_gameplayUIManager.RightChatView.AvatarInfo.avatarName);
            m_rightCardView.UpdateIcon(GameManager.Instance.GetAvatarSprite(m_gameplayUIManager.RightChatView.AvatarInfo.avatarViewId));
        }

        if (GameManager.Instance.CurrentlySelectedUser == null)
        {
#if UNITY_EDITOR
            Debug.Log("No user is selected. Select User First.");
#endif
            return;
        }

        GameManager.Instance.CurrentlySelectedUser.UserHandler.getAllAvatarsCallback.onSuccessCallbackDuringUpdateFrame += OnAvatarsRetrieved;
        StartCoroutine(GetAvatars());
    }

    private void OnDisable()
    {
        if (GameManager.Instance.CurrentlySelectedUser == null)
        {
#if UNITY_EDITOR
            Debug.Log("No user is selected.");
#endif
            return;
        }
        GameManager.Instance.CurrentlySelectedUser.UserHandler.getAllAvatarsCallback.onSuccessCallbackDuringUpdateFrame -= OnAvatarsRetrieved;
    }

    private IEnumerator GetAvatars()
    {
        yield return new WaitForSeconds(0);
        GameManager.Instance.GetAllAvatarsFromSelectedWorld();
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
                    if (m_gameplayUIManager.CanUpdateChatView(_avatar, m_isLeftSelected))
                    {
                        m_selectedCardView.UpdateName(_avatar.avatarName);
                        m_selectedCardView.UpdateIcon(GameManager.Instance.GetAvatarSprite(_avatar.avatarViewId));
                    }

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

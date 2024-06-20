using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectionPanel : MonoBehaviour
{
    private enum SelectionType
    {
        Avatar,
        World
    }

    [SerializeField, Tooltip("Avatar UI or World UI Prefab")]
    private Button m_contentPrefab;
    [SerializeField]
    private Transform m_contentContainer;
    [SerializeField]
    private SelectionType m_selectionType;
    [SerializeField]
    private UnityEvent m_onCardViewClick;

    private void OnEnable()
    {
        DestroyContents();
        if (m_selectionType == SelectionType.Avatar)
        {
            GameManager.Instance.getAllAvatarsProcess.Subscribe(OnAllAvatarsRetrieved);
            GameManager.Instance.GetAllMyAvatars();
        }
        else
        {
            GameManager.Instance.getAllWorldsProcess.Subscribe(OnAllWorldsRetrieved);
            GameManager.Instance.GetAllWorlds();
        }
    }

    private void OnDisable()
    {
        if (m_selectionType == SelectionType.Avatar)
        {
            GameManager.Instance.getAllAvatarsProcess.Unsubscribe(OnAllAvatarsRetrieved);
        }
        else
        {
            GameManager.Instance.getAllWorldsProcess.Unsubscribe(OnAllWorldsRetrieved);
        }
    }

    private void OnAllAvatarsRetrieved(List<AvatarInfo> _avatars)
    {

        foreach (var _avatar in _avatars)
        {
            Button _btn = InstantiateBtn(_avatar.avatarName, _avatar.avatarViewId);
            _btn.onClick.AddListener(() =>
            {
                GameManager.Instance.UpdateSelectedAvatar(_avatar);
                m_onCardViewClick?.Invoke();
            });
        }
    }

    private void OnAllWorldsRetrieved(List<WorldInfo> _worlds)
    {
        foreach (var _world in _worlds)
        {
            Button _btn = InstantiateBtn(_world.worldName, _world.worldViewId);
            _btn.onClick.AddListener(() =>
            {
                GameManager.Instance.UpdateSelectedWorld(_world);
                m_onCardViewClick?.Invoke();
            });
        }
    }

    private Button InstantiateBtn(string _name, string _viewId)
    {
        Button _btn = Instantiate(m_contentPrefab, m_contentContainer);
        CardView _cardView = _btn.GetComponent<CardView>();
        if (_cardView)
        {
            Sprite _icon;
            if (m_selectionType == SelectionType.Avatar)
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

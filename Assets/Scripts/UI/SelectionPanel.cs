using UnityEngine;
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

    private void Start()
    {
        GameManager.Instance.onGetAllAvatarsCompleted += () =>
        {

        };

        GameManager.Instance.onGetAllWorldsCompleted += () =>
        {

        };
    }

    private void OnEnable()
    {
        for (int i = m_contentContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(m_contentContainer.GetChild(i).gameObject);
        }

        if (m_selectionType == SelectionType.Avatar)
        {
            GameManager.Instance.GetAllAvatars();
        }
        else
        {
            GameManager.Instance.GetAllWorlds();
        }
    }
}

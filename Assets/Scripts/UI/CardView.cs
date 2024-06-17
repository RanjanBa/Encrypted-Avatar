using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField]
    private Image m_cardIcon;
    [SerializeField]
    private TMP_Text m_cardText;

    public void UpdateIcon(Sprite _icon)
    {
        m_cardIcon.sprite = _icon;
    }

    public void UpdateName(string _name)
    {
        if (string.IsNullOrEmpty(_name))
        {
            m_cardText.gameObject.SetActive(false);
            return;
        }
        m_cardText.gameObject.SetActive(true);
        m_cardText.text = _name;
    }
}
using TMPro;
using UnityEngine;

public class GameplayUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject m_selectAvatarPanel;
    [SerializeField]
    private TMP_Text m_worldNameText;

    private void OnEnable()
    {
        // m_worldNameText.text = GameManager.Instance
    }
}

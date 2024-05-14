using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get { return m_instance; } }

    private static GameManager m_instance;

    [SerializeField]
    private GameObject m_connectPanel;
    [SerializeField]
    private TMP_InputField m_playerNameInputField;
    [SerializeField]
    private Button m_connectBtn;
    [SerializeField]

    private void Awake()
    {
        if(m_instance != null)
        {
            if(m_instance.gameObject == gameObject)
            {
                Destroy(this);
            } else
            {
                Destroy(gameObject);
            }
        }

        m_instance = this;
    }

    private void Start()
    {
        m_connectBtn.onClick.AddListener(() => {
            if(m_playerNameInputField == null || m_playerNameInputField.text == "") return;

            Metaverse.Instance.CreateAvatar(m_playerNameInputField.text);
        });
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab)) {
            m_connectPanel.SetActive(!m_connectPanel.activeSelf);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Metaverse : MonoBehaviour
{
    public static Metaverse Instance { get { return m_instance; } }

    private static Metaverse m_instance;

    [SerializeField]
    private Button m_createAvatarBtn;
    [SerializeField]
    private  TMP_InputField m_avatarNameField;
    [SerializeField]
    private TMP_Dropdown m_dropdown;
    [SerializeField]
    private Button m_getKeyBtn;
    [SerializeField]
    private TMP_InputField m_msgInputField;
    [SerializeField]
    private Button m_sendBtn;
    [SerializeField]
    private Button m_decryptBtn;
    [SerializeField]
    private GameObject m_avatarPrefabs;

    private Client m_selectedClient;

    private List<Client> m_clients = new List<Client>();

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
        m_createAvatarBtn.onClick.AddListener(() =>
        {
            if(m_avatarNameField == null || m_avatarNameField.text == "")
            {
                return;
            }
            CreateAvatar(m_avatarNameField.text);
            m_avatarNameField.text = "";
        });

        m_dropdown.onValueChanged.AddListener((index) =>
        {
            string alias_name = m_dropdown.options[index].text;
            m_selectedClient = m_clients.Find((c => c.alias == alias_name));
        });

        foreach (var item in m_clients)
        {
            m_dropdown.AddOptions(new List<string>{ item.alias});
        }

        m_getKeyBtn.onClick.AddListener(() =>
        {
            if (m_selectedClient == null) return;

            m_selectedClient.GetKey();
        });

        m_sendBtn.onClick.AddListener(() =>
        {
            if (m_selectedClient == null || m_msgInputField == null || m_msgInputField.text == "") return;

            m_selectedClient.EncryptMsg(m_msgInputField.text);
        });

        m_decryptBtn.onClick.AddListener(() =>
        {
            m_selectedClient.DecryptMsg();
        });
    }

    public void CreateAvatar(string alias_name)
    {
        if(m_clients.Find((c) => c.alias == alias_name) != null)
        {
            Debug.Log("Client Already present with same name");
            return;
        }

        GameObject gm = Instantiate(m_avatarPrefabs, transform);
        gm.name = alias_name;
        Client client = gm.GetComponent<Client>();
        client.alias = alias_name;

        if(client != null)
        {
            m_clients.Add(client);
            m_dropdown.AddOptions(new List<string> { client.alias });
        } else
        {
            Destroy(gm);
        }
    }
}

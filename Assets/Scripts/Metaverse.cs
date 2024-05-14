using System.Collections.Generic;
using UnityEngine;

public class Metaverse : MonoBehaviour
{
    public static Metaverse Instance { get { return m_instance; } }

    private static Metaverse m_instance;

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
        } else
        {
            Destroy(gm);
        }
    }
}

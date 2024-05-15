using System.Collections.Generic;
using UnityEngine;

public class Metaverse : MonoBehaviour
{
    public static Metaverse Instance { get { return m_instance; } }

    private static Metaverse m_instance;

    [SerializeField]
    private GameObject m_avatarPrefabs;

    private List<Client> m_clients = new List<Client>();

    private void Awake()
    {
        if (m_instance != null)
        {
            if (m_instance.gameObject == gameObject)
            {
                Destroy(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        m_instance = this;
    }

    public void CreateAvatar(string _aliasName)
    {
        if (m_clients.Find((c) => c.alias == _aliasName) != null)
        {
            Debug.Log("Client Already present with same name");
            return;
        }

        GameObject gm = Instantiate(m_avatarPrefabs, transform);
        gm.name = _aliasName;
        Client client = gm.GetComponent<Client>();
        client.alias = _aliasName;

        if (client != null)
        {
            if (GameManager.Instance.LeftAvatarView.Avatar == null)
            {
                GameManager.Instance.LeftAvatarView.Avatar = client;
            }
            else if (GameManager.Instance.RightAvatarView.Avatar == null)
            {
                GameManager.Instance.RightAvatarView.Avatar = client;
            }
            m_clients.Add(client);
        }
        else
        {
            Destroy(gm);
        }
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[RequireComponent(typeof(Client))]
public class User : MonoBehaviour
{
    private string m_userName;
    private string m_usedId;
    private string m_privateKey;
    private string m_publicKey;

    public string UserName => m_userName;

    private Client m_mainServerClient;

    private void Start()
    {
        m_mainServerClient = GetComponent<Client>();
    }

    public void CreateAvatar(string _avatarName)
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.CREATE_AVATAR},
            {Keys.AVATAR_NAME, _avatarName}
        };
        string _msg = JsonConvert.SerializeObject(_msgDict);
        m_mainServerClient.SendMessageToServer(_msg);
    }
}

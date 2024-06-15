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
        m_mainServerClient.onAvatarCreated += (_avatarInfo) =>
        {
            GameManager.Instance.AvatarCreationCompleted();
        };

        m_mainServerClient.onWorldCreated += (_worldInfo) =>
        {
            GameManager.Instance.WorldCreationCompleted();
        };

        m_mainServerClient.onAllWorldsRetrieved += (_worlds) =>
        {
            GameManager.Instance.AvatarCreationCompleted();
        };
    }

    public void CreateAvatar(string _avatarName, string _viewId)
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.CREATE_AVATAR},
            {Keys.AVATAR_NAME, _avatarName},
            {Keys.VIEW_ID, _viewId}
        };
        string _msg = JsonConvert.SerializeObject(_msgDict);
        m_mainServerClient.SendMessageToServer(_msg);
    }

    public void CreateWorld(string _worldName, string _viewId)
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.CREATE_WORLD},
            {Keys.WORLD_NAME, _worldName},
            {Keys.VIEW_ID, _viewId}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        m_mainServerClient.SendMessageToServer(_msg);
    }

    public void GetAllAvatars()
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.ALL_AVATARS}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        m_mainServerClient.SendMessageToServer(_msg);
    }

    public void GetAllWorlds()
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.ALL_WORLDS}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        m_mainServerClient.SendMessageToServer(_msg);
    }
}

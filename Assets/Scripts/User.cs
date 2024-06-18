using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[RequireComponent(typeof(Client))]
public class User : MonoBehaviour
{
    // private string m_usedId;
    // private string m_privateKey;
    // private string m_publicKey;

    private Client m_mainServerClient;

    private void Start()
    {
        m_mainServerClient = GetComponent<Client>();
        m_mainServerClient.onAvatarCreated += (_avatarInfo) =>
        {
            GameManager.Instance.AvatarCreationCompleted(_avatarInfo);
        };

        m_mainServerClient.onWorldCreated += (_worldInfo) =>
        {
            GameManager.Instance.WorldCreationCompleted(_worldInfo);
        };

        m_mainServerClient.onAllAvatarsRetrieved += (_avatars) =>
        {
            GameManager.Instance.GetAllAvatarsCompleted(_avatars);
        };

        m_mainServerClient.onAllWorldsRetrieved += (_worlds) =>
        {
            GameManager.Instance.GetAllWorldsCompleted(_worlds);
        };

        m_mainServerClient.onWorldJoined += (_info) =>
        {
            GameManager.Instance.WorldJoinnedCompleted(_info);
        };
    }

    public void CreateAvatar(string _avatarName, string _viewId)
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.CREATE_AVATAR},
            {Keys.AVATAR_NAME, _avatarName},
            {Keys.AVATAR_VIEW_ID, _viewId}
        };
        string _msg = JsonConvert.SerializeObject(_msgDict);
        m_mainServerClient.SendMessageToServer(_msg);
    }

    public void CreateWorld(string _worldName, string _viewId)
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.CREATE_WORLD},
            {Keys.WORLD_NAME, _worldName},
            {Keys.WORLD_VIEW_ID, _viewId}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        m_mainServerClient.SendMessageToServer(_msg);
    }

    public void GetAllMyAvatars()
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.CLIENT_ALL_AVATARS}
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

    public void JoinWorld(string _avatarId, string _worldId)
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.JOIN_WORLD},
            {Keys.AVATAR_ID, _avatarId},
            {Keys.WORLD_ID, _worldId}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        m_mainServerClient.SendMessageToServer(_msg);
    }

    public void GetAllAvatarsFromWorld(string _worldId)
    {
        Dictionary<string, string> _msgDict = new Dictionary<string, string>() {
            {Keys.INSTRUCTION, Instructions.WORLD_ALL_AVATARS},
            {Keys.WORLD_ID, _worldId}
        };

        string _msg = JsonConvert.SerializeObject(_msgDict);
        m_mainServerClient.SendMessageToServer(_msg);
    }
}

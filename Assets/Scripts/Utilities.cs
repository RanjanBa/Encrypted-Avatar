using System;
using UnityEngine;

public static class Instructions
{
    public const string REGISTER_USER = "register_user";
    public const string LOGIN_USER = "login_user";
    public const string CREATE_AVATAR = "create_avatar";
    public const string CLIENT_ALL_AVATARS = "client_all_avatars";
    public const string CREATE_WORLD = "create_world";
    public const string JOIN_WORLD = "join_world";
    public const string WORLD_ALL_AVATARS = "world_all_avatars";
    public const string SEND_MSG = "send_msg";
    public const string ALL_WORLDS = "all_worlds";
    public const string SERVER_KEY = "server_key";
    public const string CLIENT_KEY = "client_key";
    public const string AVATAR_KEY = "avatar_key";
    public const string GET_CLIENT_KEY = "get_client_key";
    public const string GENERATE_KEY = "generate_key";
    public const string ENCRYPT_MSG = "encrypt_msg";
    public const string DECRYPT_MSG = "decrypt_msg";
}

public static class Keys
{
    public const string MSG_TYPE = "msg_type";
    public const string INSTRUCTION = "instruction";
    public const string ERROR = "error";
    public const string MESSAGE = "msg";
    public const string WORLD_ID = "world_id";
    public const string WORLD_NAME = "world_name";
    public const string WORLD_VIEW_ID = "world_view_id";
    public const string AVATAR_ID = "avatar_id";
    public const string AVATAR_NAME = "avatar_name";
    public const string AVATAR_VIEW_ID = "avatar_view_id";
    public const string RECIEVER_ID = "receiver_id";
    public const string PUBLIC_KEY = "public_key";
    public const string PRIVATE_KEY = "private_key";
    public const string ENCAPSULATED_KEY = "encapsulated_key";
    public const string CIPHER_TEXT = "cipher_text";
    public const string TAG = "tag";
    public const string NONCE = "nonce";
    public const string REGISTRATION_INFO = "registration_info";
    public const string LOGIN_INFO = "login_info";
    public const string USER_ID = "user_id";
    public const string FIRST_NAME = "first_name";
    public const string LAST_NAME = "last_name";
    public const string USER_NAME = "user_name";
    public const string PASSWORD = "password";
}

public static class MessageType
{
    public const string PLAIN_TEXT = "plain_text";
    public const string ENCRYPTED_TEXT = "encrypted_text";
}

public enum CallbackStatus
{
    None,
    Pending,
    Success,
    Failure
}

[Serializable]
public class IconWithID
{
    public string viewId;
    public Sprite icon;
    public GameObject prefab;
}

public class AvatarAndWorldInfo
{
    public AvatarInfo avatarInfo;
    public WorldInfo worldInfo;
}

public class ToastMsg
{
    public string msg;
    public float duration;

    public ToastMsg(string _msg, float _duration)
    {
        msg = _msg;
        duration = _duration;
    }
}

public static class Utility
{
    public static string Truncate(this string _str, int _maxChars)
    {
        return _str.Length <= _maxChars ? _str : _str.Substring(0, _maxChars - 3) + "...";
    }
}
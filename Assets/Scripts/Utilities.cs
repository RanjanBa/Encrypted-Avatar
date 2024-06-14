using System.Collections.Generic;
using UnityEngine;

public static class Instructions
{
    public const string CREATE_AVATAR = "create_avatar";
    public const string AVATAR_INFO = "avatar_info";
    public const string CREATE_WORLD = "create_world";
    public const string JOIN_WORLD = "join_world";
    public const string SEND_MSG = "send_msg";
    public const string SENT_KEY = "sent_key";
    public const string WORLD_INFO = "world_info";
    public const string ALL_WORLDS = "all_worlds";
    public const string ENCRYPT_MSG = "encrypt_msg";
    public const string DECRYPT_MSG = "decrypt_msg";
    public const string GENERATE_KEY = "generate_key";
}

public static class Keys
{
    public const string INSTRUCTION = "instruction";
    public const string MESSAGE = "msg";
    public const string RECIEVER = "receiver";
    public const string WORLD_ID = "world_id";
    public const string WORLD_NAME = "world_name";
    public const string AVATAR_ID = "avatar_id";
    public const string AVATAR_NAME = "avatar_name";
    public const string PUBLIC_KEY = "public_key";
    public const string PRIVATE_KEY = "private_key";
    public const string STATUS = "status";
    public const string ENC_SESSION_KEY = "enc_session_key";
    public const string TAG = "tag";
    public const string CIPHER_TEXT = "cipher_text";
    public const string NONCE = "nonce";
}

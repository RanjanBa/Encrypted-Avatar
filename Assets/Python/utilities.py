from enum import Enum


class Instructions(Enum):
    CREATE_AVATAR = "create_avatar"
    AVATAR_INFO = "avatar_info"
    CREATE_WORLD = "create_world"
    JOIN_WORLD = "join_world"
    SEND_MSG = "send_msg"
    SENT_KEY = "sent_key"
    WORLD_INFO = "world_info"
    ALL_WORLDS = "all_worlds"
    ENCRYPT_MSG = "encrypt_msg"
    DECRYPT_MSG = "decrypt_msg"
    GENERATE_KEY = "generate_key"
    
class Keys(Enum):
    INSTRUCTION = "instruction"
    MESSAGE = "msg"
    RECIEVER = "receiver"
    WORLD_ID = "world_id"
    WORLD_NAME = "world_name"
    AVATAR_ID = "avatar_id"
    AVATAR_NAME = "avatar_name"
    PUBLIC_KEY = "public_key"
    PRIVATE_KEY = "private_key"
    STATUS = "status"
    ENC_SESSION_KEY = "enc_session_key"
    TAG = "tag"
    CIPHER_TEXT = "cipher_text"
    NONCE = "nonce"
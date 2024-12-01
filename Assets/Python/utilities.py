from enum import Enum


class ServerInstructions(Enum):
    GET_KEY = "get_key"
    SET_KEY = "set_key"
    GET_CLIENT_ALL_AVATARS = "get_client_all_avatars"
    GET_WORLD_ALL_AVATARS = "get_world_all_avatars"
    GET_ALL_WORLDS = "all_worlds"
    CREATE_AVATAR = "create_avatar"
    CREATE_WORLD = "create_world"
    LOGIN_USER = "login_user"
    JOIN_WORLD = "join_world"
    REGISTER_USER = "register_user"
    SEND_MSG = "send_msg"

class LocalInstructions(Enum):
    ENCRYPT_MSG = "encrypt_msg"
    DECRYPT_MSG = "decrypt_msg"
    GENERATE_KEY = "generate_key"

class Keys(Enum):
    MSG_TYPE = "msg_type"
    INSTRUCTION = "instruction"
    ERROR = "error"
    MESSAGE = "msg"
    WORLD_ID = "world_id"
    WORLD_NAME = "world_name"
    WORLD_VIEW_ID = "world_view_id"
    AVATAR_ID = "avatar_id"
    AVATAR_NAME = "avatar_name"
    AVATAR_VIEW_ID = "avatar_view_id"
    RECIEVER_ID = "receiver_id"
    PUBLIC_KEY = "public_key"
    PRIVATE_KEY = "private_key"
    ENCAPSULATED_KEY = "encapsulated_key"
    TAG = "tag"
    CIPHER_TEXT = "cipher_text"
    NONCE = "nonce"
    REGISTRATION_INFO = "registration_info"
    LOGIN_INFO = "login_info"
    USER_ID = "user_id"
    FIRST_NAME = "first_name"
    LAST_NAME = "last_name"
    USER_NAME = "user_name"
    PASSWORD = "password"
    
class MessageType(Enum):
    PLAIN_TEXT = "plain_text"
    ENCRYPTED_TEXT = "encrypted_text"
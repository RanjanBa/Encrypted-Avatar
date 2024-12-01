from enum import Enum


class ServerInstructions(Enum):
    GET_SERVER_KEY = "get_server_key"
    SET_CLIENT_KEY = "set_client_key"
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
    SIGN_MSG = "sign_msg"
    VERIFY_MSG = "verify_msg"
    GENERATE_KEY = "generate_key"

class Keys(Enum):
    MSG_TYPE = "msg_type"
    INSTRUCTION = "instruction"
    KYBER_PUBLIC_KEY = "kyber_public_key"
    KYBER_PRIVATE_KEY = "kyber_private_key"
    DILITHIUM_PUBLIC_KEY = "dilithium_public_key"
    DILITHIUM_PRIVATE_KEY = "dilithium_private_key"
    ERROR = "error"
    MESSAGE = "msg"
    WORLD_ID = "world_id"
    WORLD_NAME = "world_name"
    WORLD_VIEW_ID = "world_view_id"
    AVATAR_ID = "avatar_id"
    AVATAR_NAME = "avatar_name"
    AVATAR_VIEW_ID = "avatar_view_id"
    RECIEVER_ID = "receiver_id"
    ENCAPSULATED_KEY = "encapsulated_key"
    TAG = "tag"
    CIPHER_TEXT = "cipher_text"
    NONCE = "nonce"
    SIGNATURE = "signature"
    VERIFICATION_STATUS = "verification_status" 
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

class VerificationStatus(Enum):
    VERIFIED = "verified"
    UNVERIFIED = "unverified"
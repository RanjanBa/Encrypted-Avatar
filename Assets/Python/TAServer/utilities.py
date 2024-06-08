from enum import Enum


class Instructions(Enum):
    CREATE_AVATAR = "create_avatar"
    AVATAR_INFO = "avatar_info"
    CREATE_WORLD = "create_world"
    JOIN_WORLD = "join_world"
    SEND = "send_msg"
    WORLD_INFO = "world_info"
    ALL_WORLDS = "all_worlds"
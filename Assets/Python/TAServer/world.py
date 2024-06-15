from avatar import Avatar

class World:
    def __init__(self, _id : str, _name : str, _view_id : str) -> None:
        self.__world_id : str = _id
        self.__world_name : str = _name
        self.__view_id : str = _view_id
        self.__avatarsDict : dict[str, Avatar] = {}

    @property
    def worldId(self):
        return self.__world_id
    
    @property
    def worldName(self):
        return self.__world_name
    
    @property
    def ViewId(self):
        return self.__view_id
    
    @property
    def avatars(self) -> list[Avatar]:
        return list(self.__avatarsDict.values())
    
    def addAvatar(self, _avatar : Avatar):
        if _avatar.avatarID in self.__avatarsDict:
            return False
        self.__avatarsDict[_avatar.avatarID] = _avatar
        return True

    def removeAvatar(self, _avatar : Avatar):
        if not self.__avatarsDict.__contains__(_avatar.avatarID):
            return False
        self.__avatarsDict.pop(_avatar.avatarID)
        return True
    
    def __contains__(self, _avatar : Avatar):
        if not _avatar.avatarID in self.__avatarsDict:
            return False

        return True

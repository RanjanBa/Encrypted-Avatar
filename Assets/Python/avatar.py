class Avatar:
    def __init__(self, _avatar_id : str, _avatar_name : str) -> None:
        self.__avatar_id = _avatar_id
        self.__avatar_name = _avatar_name
        
    @property
    def avatarName(self):
        return self.__avatar_name
    
    @property
    def avatarID(self):
        return self.__avatar_id
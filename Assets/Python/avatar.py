class Avatar:
    def __init__(self, _avatar_id : str, _avatar_name : str, _view_id : str) -> None:
        self.__avatar_id = _avatar_id
        self.__avatar_name = _avatar_name
        self.__view_id = _view_id
        
    @property
    def avatarName(self):
        return self.__avatar_name
    
    @property
    def avatarID(self):
        return self.__avatar_id
    
    @property
    def viewId(self):
        return self.__view_id
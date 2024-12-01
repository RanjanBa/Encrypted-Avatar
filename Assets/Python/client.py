from socket import socket
from avatar import Avatar


class Client:
    def __init__(self, _socket : socket) -> None:
        self.__socket = _socket
        self.__avatars_dict : dict[str, Avatar] = {}
        self.__public_key : str = None

    @property
    def socket(self):
        return self.__socket
    
    @property
    def avatars(self) -> list[Avatar]:
        return list(self.__avatars_dict.values())
    
    @property
    def publicKey(self):
        return self.__public_key
    
    @publicKey.setter
    def publicKey(self, pk):
        self.__public_key = pk
    
    def close(self):
        self.__socket.close()
        self.__socket = None
    
    def sendMessage(self, msg : str):
        if self.__socket == None:
            print("Not connected to the client...")
            return

        self.__socket.send(msg.encode())
        
    def receiveMessage(self, bufferSize : int = 1024) -> str:
        if self.__socket == None:
            print("Not connected to the client...")
            return None
        
        encoded_msg = self.__socket.recv(bufferSize)
        return encoded_msg.decode()
        
    def addAvatar(self, _avatar : Avatar) -> bool:
        if _avatar.avatarID in self.__avatars_dict:
            return False
        
        self.__avatars_dict[_avatar.avatarID] = _avatar
        return True
    
    def removeAvatar(self, _avatar : Avatar) -> bool:
        if not _avatar.avatarID in self.__avatars_dict:
            return False
        self.__avatars_dict.pop(_avatar.avatarID)
        return True
    
    def getAvatar(self, _avatar_id : str) -> Avatar:
        if not _avatar_id in self.__avatars_dict:
            return None
        
        return self.__avatars_dict[_avatar_id]
    
    def __contains__(self, _avatar : Avatar):
        if not _avatar.avatarID in self.__avatars_dict:
            return False

        return True
        
        
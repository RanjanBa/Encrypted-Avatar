from socket import socket
from avatar import Avatar


class Client:
    def __init__(self, _socket : socket) -> None:
        print("Client is created...")
        self.__socket = _socket
        self.__avatars : list[Avatar] = []

    @property
    def socket(self):
        return self.__socket
    
    @property
    def avatars(self):
        return self.__avatars
    
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
        
    def addAvatar(self, _avatar : Avatar):
        for _av in self.__avatars:
            if _av.avatarID == _avatar.avatarID:
                return False
        
        self.__avatars.append(_avatar)
        return True
        
        
from client import Client

class World:
    def __init__(self, _id : int) -> None:
        self.__world_id : int = _id
        self.__clients : list[Client] = []

    def addClient(self, _client : Client):
        self.__clients.append(_client)

    def removeClient(self, _client : Client):
        self.__clients.remove(_client) 
    

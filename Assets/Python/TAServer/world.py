from client import Client

class World:
    def __init__(self, _id : str, _name : str) -> None:
        self.__world_id : str = _id
        self.__world_name : str = _name
        self.__clients : list[Client] = []

    def addClient(self, _client : Client):
        self.__clients.append(_client)

    def removeClient(self, _client : Client):
        self.__clients.remove(_client)
    
    @property
    def worldId(self):
        return self.__world_id
    
    @property
    def worldName(self):
        return self.__world_name
    
    @property
    def clients(self):
        return self.__clients

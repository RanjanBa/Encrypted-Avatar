import sys

sys.path.append('./Kyber')

from socket import socket, AF_INET, SOCK_STREAM
from threading import Thread
from client import Client
from world import World
from avatar import Avatar
from utilities import Instructions
from kyber import Kyber1024
import uuid

host = "127.0.0.1"
port = 12000

main_server = socket(AF_INET, SOCK_STREAM)
main_server.bind((host, port))
main_server.listen(5)

clients : set[Client] = set()
worldsDict : dict[str, World] = {}

pk, sk = Kyber1024.keygen()


def getClient(address, port) -> Client:
    for client in clients:
        h, p = client.socket.getpeername()
        if h == address and p == port:
            return client
        
    return None


def createAvatar(client : Client, sentences : list[str]):
    avatar_name = sentences[0]
    avatar_id = str(uuid.uuid4())
    
    avatar = Avatar(avatar_id, avatar_name)
    
    if client.addAvatar(avatar):
        print(f"New avatar is created with id : {avatar_id} , name : {avatar_name}")
    else:
        print(f"Avatar can't be created with id {avatar_id}. Client already have that avatar")


def createNewWorld(client : Client, sentences : list[str]):
    if len(sentences) < 1:
        print("Can't create new world. All parameters are not given.")
        return

    world_id = str(uuid.uuid4())
    if worldsDict.__contains__(world_id):
        print(f"Can't create new world. World is already exist with world id {world_id}.")
        client.sendMessage(f"Can't create new world. World is already exist with world id {world_id}.")
    
    world_name = sentences[0].strip()
    
    world = World(world_id, world_name)
    world.addClient(client)
    worldsDict[world_id] = world
    client.sendMessage(f"World is created with world id {world_id}.")


def joinWorld(client : Client, sentences : list[str]):
    world_id = sentences[0].strip()
    
    if not worldsDict.__contains__(world_id):
        print(f"Can't join the world with id {world_id}. World with that id doesn't exists.")
        client.sendMessage(f"Can't join the world with id {world_id}. World with that id doesn't exists.")
        return
    
    if client in worldsDict[world_id].clients:
        print(f"You already joinned the world with id {world_id}.")
        client.sendMessage(f"You already joinned the world with id {world_id}.")
        return
    
    worldsDict[world_id].addClient(client)
    client.sendMessage(f"You joinned to the world with id {world_id}")


def sendMessage(client : Client, sentences : list[str]):
    h, p = sentences[0].split()
    receiver : Client = getClient(h, int(p))
    msg = ""
    for s in sentences[1:]:
        msg += s
    
    if receiver != None:
        if receiver != client:
            receiver.sendMessage(msg)
        else:
            print("Can't send to it self...")
    else:
        print("Can't find receiver")
        client.sendMessage("Can't find receiver client")


def allWorlds(client : Client, sentences : list[str]):
    msg = "all_worlds\n"
    
    for k, v in worldsDict:
        msg += "(" + k + " , " + v + ")\n"
    
    print(msg)
    client.sendMessage(msg)


def worldInfo(client : Client, sentences : list[str]):
    world_id = sentences[0].strip()

    if not worldsDict.__contains__(world_id):
        print(f"Can't get info of the world with id {world_id}. World with that id doesn't exists.")
        client.sendMessage(f"Can't get info of the world with id {world_id}. World with that id doesn't exists.")
        return

    world_clients = worldsDict[world_id].clients
    msg = "world_infos\n"
    msg += "total clients : " + str(len(world_clients)) + "\n"
    for idx in range(len(world_clients)):
        msg += str(world_clients[idx].socket.getpeername())
        if idx < len(world_clients) - 1:
            msg += "\n"
    
    print(msg)
    client.sendMessage(msg)


def clientAvatarInfo(client : Client, sentences : list[str]):
    msg = ""
    for idx, avatar in enumerate(client.avatars):
        msg += "(" + avatar.avatarName + " , " + avatar.avatarID + ")"
        if idx < len(client.avatars) - 1:
            msg += "\n"
    
    print(msg)
    client.sendMessage(msg)


def parseMessage(msg : str, client : Client):
    sentences = msg.split('\n')

    if len(sentences) == 0:
        print("Message is sent with whitespaces...")
        return

    instruction = sentences[0].strip()

    if instruction == Instructions.CREATE_AVATAR.value:
        createAvatar(client, sentences[1:])
    elif instruction == Instructions.AVATAR_INFO.value:
        clientAvatarInfo(client, sentences[1:])
    elif instruction == Instructions.CREATE_WORLD.value:
        createNewWorld(client, sentences[1:])
    elif instruction == Instructions.JOIN_WORLD.value:
        joinWorld(client, sentences[1:])
    elif instruction == Instructions.SEND.value:
        sendMessage(client, sentences[1:])
    elif instruction == Instructions.ALL_WORLDS.value:
        allWorlds(client, sentences[1:])
    elif instruction == Instructions.WORLD_INFO.value:
        worldInfo(client, sentences[1:])
    else:
        print("Message is sent without any instruction...")


def handleClient(client_socket : socket):
    h, p = client_socket.getpeername()
    client = getClient(h, p)
    
    if client != None:
        print("Already joinned...")
        client_socket.send("You are already joinned...".encode())
    
    client = Client(client_socket)
    clients.add(client)
    print(f"After adding new client, Number of clients {len(clients)}.")
    
    try:
        client.sendMessage(f"Hello Client!\npublic_key: {bytes.hex(pk)}")
        msg = client.receiveMessage(1024)
        print(msg)
    except:
        print("Some error in receiving or sending data...")
        clients.remove(client)
        client.close()
        print(f"Number of clients remaining {len(clients)}.")
        return

    while True:
        try:
            msg = client.receiveMessage(1024)
            if msg == None or msg == '':
                print("No message is sent")
                client.close()
                break
            print(msg)
            parseMessage(msg, client)
        except KeyboardInterrupt:
            client.close()
            break
    
    clients.remove(client)
    print(f"Number of clients remaining {len(clients)}.")


def startServer():
    while True:
        try:
            print("Server is listening and waiting for new client...")
            client_socket, address = main_server.accept()
            print(f"New client of address {address} is connected with server {main_server.getsockname()}.")
            thread = Thread(target=handleClient, args=(client_socket,))
            thread.daemon = True
            thread.start()
        except KeyboardInterrupt:
            print("Keyboard is interrupted.")
            main_server.close()
            break
        except:
            print("Unknown error!")
            main_server.close()
            break


if __name__ == "__main__":
    startServer()
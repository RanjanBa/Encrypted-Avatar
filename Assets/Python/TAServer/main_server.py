import sys

sys.path.append('./')
sys.path.append('./Kyber')

from socket import socket, AF_INET, SOCK_STREAM
from threading import Thread
from client import Client
from world import World
from avatar import Avatar
from utilities import Instructions, Keys
from kyber import Kyber1024
import json
import uuid

DATA_BUFFER_SIZE = 10240
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


def createAvatar(client : Client, parsedMsg : dict):
    if not Keys.AVATAR_NAME.value in parsedMsg:
        print("No avatar name key is present in the msg.")
        return

    avatar_name = parsedMsg[Keys.AVATAR_NAME.value]
    avatar_id = str(uuid.uuid4())
    
    avatar = Avatar(avatar_id, avatar_name)
    
    if client.addAvatar(avatar):
        print(f"New avatar is created with id : {avatar_id} , name : {avatar_name}")
        client.sendMessage(f"New avatar is created with id : {avatar_id} , name : {avatar_name}")
    else:
        print(f"Avatar can't be created with id {avatar_id}. Client already have that avatar")
        client.sendMessage(f"Avatar can't be created with id {avatar_id}. Client already have that avatar")


def createNewWorld(client : Client, parsedMsg : dict):
    if not Keys.WORLD_NAME.value in parsedMsg:
        print("No world name key is present in the msg.")
        return

    world_name = parsedMsg[Keys.WORLD_NAME.value]    
    
    world_id = str(uuid.uuid4())
    if worldsDict.__contains__(world_id):
        print(f"Can't create new world. World is already exist with world id {world_id}.")
        client.sendMessage(f"Can't create new world. World is already exist with world id {world_id}.")
    
    world = World(world_id, world_name)
    worldsDict[world_id] = world
    client.sendMessage(f"World is created with world id {world_id}.")


def joinWorld(client : Client, parsedMsg : dict):
    if not Keys.WORLD_ID.value in parsedMsg:
        print("No world id key is present in the msg.")
        return

    if not Keys.AVATAR_ID.value in parsedMsg:
        print("No avatar id key is present in the msg.")
        return

    world_id = parsedMsg[Keys.WORLD_ID.value]
    if not worldsDict.__contains__(world_id):
        print(f"Can't join the world with id {world_id}. World with that id doesn't exists.")
        client.sendMessage(f"Can't join the world with id {world_id}. World with that id doesn't exists.")
        return
    
    avatar_id = parsedMsg[Keys.AVATAR_ID.value]
    
    avatar = client.getAvatar(avatar_id)
    if avatar in worldsDict[world_id]:
        print(f"{avatar.avatarName} has already joinned the world with id {world_id}.")
        client.sendMessage(f"You have already joinned the world with id {world_id}.")
        return
    
    worldsDict[world_id].addAvatar(avatar)
    client.sendMessage(f"You join to the world with id {world_id}")


def sendMessage(client : Client, parsedMsg : dict):
    if not Keys.AVATAR_ID.value in parsedMsg:
        print("No avatar id key is present in the msg.")
        return

    if not Keys.MESSAGE.value in parsedMsg:
        print("No message key is present in the msg.")
        return

    avatar_id = parsedMsg[Keys.AVATAR_ID.value]
    
    receiver : Client = None
    for c in clients:
        av = c.getAvatar(avatar_id)
        if av != None:
            print("FOUND RECEIVER...")
            receiver = c
            break

    msg = parsedMsg[Keys.MESSAGE.value]
    
    if receiver != None:
        if receiver != client:
            receiver.sendMessage(msg)
        else:
            print("Can't send to itself...")
            client.sendMessage("Can't send to itself...")
    else:
        print("Can't find receiver")
        client.sendMessage("Can't find receiver client")


def allWorlds(client : Client, parsedMsg : dict):
    msg = "all_worlds\n"
    print(type(worldsDict))
    
    for k in worldsDict:
        msg += "(" + k + " , " + worldsDict[k].worldName + ")\n"
    
    print(msg)
    client.sendMessage(msg)


def worldInfo(client : Client, parsedMsg : dict):
    if not Keys.WORLD_ID.value in parsedMsg:
        print("No world id key is present in the msg.")
        return
    
    world_id = parsedMsg[Keys.WORLD_ID.value]

    if not worldsDict.__contains__(world_id):
        print(f"Can't get info of the world with id {world_id}. World with that id doesn't exists.")
        client.sendMessage(f"Can't get info of the world with id {world_id}. World with that id doesn't exists.")
        return

    world_avatars = worldsDict[world_id].avatars
    msg = "world_infos\n"
    msg += "total avatars : " + str(len(world_avatars)) + "\n"
    for idx in range(len(world_avatars)):
        msg += str(world_avatars[idx].avatarName)
        if idx < len(world_avatars) - 1:
            msg += "\n"
    
    print(msg)
    client.sendMessage(msg)


def clientAvatarInfo(client : Client, parsedMsg : dict):
    msg = ""
    for idx, avatar in enumerate(client.avatars):
        msg += "(" + avatar.avatarName + " , " + avatar.avatarID + ")"
        if idx < len(client.avatars) - 1:
            msg += "\n"
    
    print(msg)
    client.sendMessage(msg)


def parseMessage(msg : str, client : Client):
    parsedMsg : dict = json.loads(msg)
    
    if not Keys.INSTRUCTION.value in parsedMsg:
        print("No instruction is given with the msg...")
        return

    msg_code = parsedMsg[Keys.INSTRUCTION.value]

    if msg_code == Instructions.CREATE_AVATAR.value:
        createAvatar(client, parsedMsg)
    elif msg_code == Instructions.AVATAR_INFO.value:
        clientAvatarInfo(client, parsedMsg)
    elif msg_code == Instructions.CREATE_WORLD.value:
        createNewWorld(client, parsedMsg)
    elif msg_code == Instructions.JOIN_WORLD.value:
        joinWorld(client, parsedMsg)
    elif msg_code == Instructions.ALL_WORLDS.value:
        allWorlds(client, parsedMsg)
    elif msg_code == Instructions.WORLD_INFO.value:
        worldInfo(client, parsedMsg)
    elif msg_code == Instructions.SEND_MSG.value:
        sendMessage(client, parsedMsg)
    else:
        print(f"Message is sent without proper instruction -> {msg_code}")


def handleClient(client_socket : socket):
    h, p = client_socket.getpeername()
    client = getClient(h, p)
    
    if client != None:
        client.sendMessage("You are already joinned...")
        print(f"Already joinned. Number of clients joinned {len(clients)}")
    else:
        client = Client(client_socket)
        clients.add(client)
        client.sendMessage("You join newly...")
        print(f"After adding new client, Number of clients {len(clients)}.")
    
    try:
        client.sendMessage(f"public_key:{bytes.hex(pk)}")
        client_sk = client.receiveMessage(DATA_BUFFER_SIZE)
        print(client_sk)
        while True:
            try:
                msg = client.receiveMessage(DATA_BUFFER_SIZE)
                if msg == None or msg == '':
                    print("No message is sent")
                    client.close()
                    break
                print(msg)
                parseMessage(msg, client)
            except KeyboardInterrupt:
                client.close()
                break
    except:
        print("Some error in receiving or sending data...")
        client.close()
    
    for av in client.avatars:
        for w in worldsDict.values():
            w.removeAvatar(av)
    
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
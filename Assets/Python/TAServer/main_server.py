import sys

sys.path.append('./Kyber')

from socket import socket, AF_INET, SOCK_STREAM
from threading import Thread
from client import Client
from world import World
from kyber import Kyber1024

host = "127.0.0.1"
port = 12000

main_server = socket(AF_INET, SOCK_STREAM)
main_server.bind((host, port))
main_server.listen(5)

clients : dict[Client, list[World]] = {}
worlds : dict[str, World] = {}

pk, sk = Kyber1024.keygen()


def getClient(address, port) -> Client:
    for client, _ in clients:
        h, p = client.socket.getpeername()
        if h == address and p == port:
            return client
        
    return None


def createNewWorld(client : Client, sentences : list[str]):
    world_id = sentences[0].strip()
    if worlds.__contains__(world_id):
        print(f"Can't create new world. World is already exist with world id {world_id}")
        client.sendMessage(f"Can't create new world. World is already exist with world id {world_id}")
    
    world = World(world_id)
    worlds[world_id] = world
    clients[client].append(world)
    client.sendMessage(f"World is created with world id {world_id}")


def joinWorld(client : Client, sentences : list[str]):
    world_id = sentences[0].strip()
    
    if not worlds.__contains__(world_id):
        print(f"Can't join the world with world id {world_id}. World with that id doesn't exists")
        client.sendMessage(f"Can't join the world with world id {world_id}. World with that id doesn't exists")
        return
        
    worlds[world_id].addClient(client)
    clients[client].append(worlds[world_id])
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
    
    for k in worlds.keys():
        msg += k + "\n"
    
    print(msg)
    client.sendMessage(msg)


def worldInfo(client : Client, sentences : list[str]):
    msg = "world_infos\n"
    world_id = sentences[0].strip()
    for cl in worlds[world_id].clients:
        msg += str(cl.socket.getpeername())
        
    print(msg)
    client.sendMessage(msg)


def parseMessage(msg : str, client : Client):
    if msg == None or msg == '':
        print("No message is sent")
        return
    
    print(msg)

    sentences = msg.split('\n')

    if len(sentences) == 0:
        print("Message is sent with whitespaces...")
        return

    instruction = sentences[0].strip()

    if instruction == 'create':
        createNewWorld(client, sentences[1:])
    elif instruction == 'join':
        joinWorld(client, sentences[1:])
    elif instruction == 'send':
        sendMessage(client, sentences[1:])
    elif instruction == 'all_worlds':
        allWorlds(client, sentences[1:])
    elif instruction == 'world_info':
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
    clients[client] = []
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
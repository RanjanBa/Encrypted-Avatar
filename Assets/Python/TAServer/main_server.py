import sys

sys.path.append('./')
sys.path.append('./Kyber')

from socket import socket, AF_INET, SOCK_STREAM
from threading import Thread
from client import Client
from world import World
from avatar import Avatar
from utilities import Instructions, Keys, MessageType
from kyber import Kyber1024
import kyber_encrypt_decrypt 
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

public_key, private_key = Kyber1024.keygen()


def getClientFromAddress(address, port) -> Client:
    for client in clients:
        h, p = client.socket.getpeername()
        if h == address and p == port:
            return client
        
    return None


def getClientFromAvatarId(avatar_id) -> Client:
    for client in clients:
        av = client.getAvatar(avatar_id)
        if av != None:
            return client

    return None


def DecodeInstruction(client : Client, parsedMsg : dict):
    if not Keys.INSTRUCTION.value in parsedMsg:
        print("No instruction is given with the msg...")
        return
    print("Parsing Message Complete")
    msg_code = parsedMsg[Keys.INSTRUCTION.value]
    
    if msg_code == Instructions.CLIENT_KEY.value:
        pk = parsedMsg[Keys.PUBLIC_KEY.value]
        client.publicKey = pk
        print("client public key -> " + pk)
    elif msg_code == Instructions.CREATE_AVATAR.value:
        createAvatar(client, parsedMsg)
    elif msg_code == Instructions.CLIENT_ALL_AVATARS.value:
        clientAllAvatars(client, parsedMsg)
    elif msg_code == Instructions.CREATE_WORLD.value:
        createNewWorld(client, parsedMsg)
    elif msg_code == Instructions.JOIN_WORLD.value:
        joinWorld(client, parsedMsg)
    elif msg_code == Instructions.ALL_WORLDS.value:
        allWorlds(client, parsedMsg)
    elif msg_code == Instructions.WORLD_ALL_AVATARS.value:
        worldAllAvatars(client, parsedMsg)
    elif msg_code == Instructions.SEND_MSG.value:
        sendMessage(client, parsedMsg)
    else:
        print(f"Message is sent without proper instruction -> {msg_code}")


def decryptMsg(client : Client, parsedMsg : dict):
    print("Decrypting msg...")
    enc_session_key = parsedMsg[Keys.ENC_SESSION_KEY.value]
    tag = parsedMsg[Keys.TAG.value]
    cipher_text = parsedMsg[Keys.CIPHER_TEXT.value]
    nonce = parsedMsg[Keys.NONCE.value]

    if private_key == "" or enc_session_key == "" or tag == "" or cipher_text == "" or nonce == "":
        print("Invalid private key or enc_session_key or tag or ciphertext or nonce!")
        return

    enc_session_key = bytes.fromhex(enc_session_key)
    tag = bytes.fromhex(tag)
    cipher_text = bytes.fromhex(cipher_text)
    nonce = bytes.fromhex(nonce)
    
    decrypted_msg = kyber_encrypt_decrypt.decrypt(private_key, enc_session_key, tag, cipher_text, nonce)
    print("decrypted msg...")
    print(decrypted_msg)
    parsedMsg : dict[str, str] = json.loads(decrypted_msg)
    DecodeInstruction(client, parsedMsg)


def encryptMsg(msg : str, key : str) -> dict[str, str]:
    enc_session_key, tag, cipher_text, nonce = kyber_encrypt_decrypt.encrypt(msg, bytes.fromhex(key))  
    info : dict[str, str] = {}
    info[Keys.MSG_TYPE.value] = MessageType.ENCRYPTED_TEXT.value
    info[Keys.ENC_SESSION_KEY.value] = bytes.hex(enc_session_key)
    info[Keys.TAG.value] = bytes.hex(tag)
    info[Keys.CIPHER_TEXT.value] = bytes.hex(cipher_text)
    info[Keys.NONCE.value] = bytes.hex(nonce)
    return info
    

def registerNewUser(client: Client, parsedMsg : dict):
    pass


def createAvatar(client : Client, parsedMsg : dict):
    print("Creating New Avatar")
    info : dict[str, str] = {}
    if not Keys.AVATAR_NAME.value in parsedMsg:
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = "You did not provide any avatar name in the msg."
        msg = json.dumps(info)
        client.sendMessage(msg)
        print("No avatar name key is present in the msg.")
        return

    avatar_name = parsedMsg[Keys.AVATAR_NAME.value]
    avatar_view_id = parsedMsg[Keys.AVATAR_VIEW_ID.value]
    avatar_id = str(uuid.uuid4())
    
    avatar = Avatar(avatar_id, avatar_name, avatar_view_id)
    
    if client.addAvatar(avatar):
        info[Keys.INSTRUCTION.value] = Instructions.CREATE_AVATAR.value
        info[Keys.AVATAR_ID.value] = avatar_id
        info[Keys.AVATAR_NAME.value] = avatar_name
        info[Keys.AVATAR_VIEW_ID.value] = avatar_view_id
        print(f"New avatar is created with id : {avatar_id} , name : {avatar_name}")
    else:
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = f"Avatar can't be created with id {avatar_id}. Client already have that avatar id"
        print(f"Avatar can't be created with id {avatar_id}. Client already have that avatar")
    
    msg = json.dumps(info)
    print(msg)
    encrypted_msg = encryptMsg(msg, client.publicKey)
    msg = json.dumps(encrypted_msg)
    client.sendMessage(msg)


def createNewWorld(client : Client, parsedMsg : dict):
    print("Creating New World")
    info : dict[str, str] = {}
    if not Keys.WORLD_NAME.value in parsedMsg:
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = "You did not provide any world name in the msg."
        msg = json.dumps(info)
        client.sendMessage(msg)
        print("No world name key is present in the msg.")
        return
    
    world_id = str(uuid.uuid4())
    if worldsDict.__contains__(world_id):
        info = {}
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = f"Can't create new world. World is already exist with world id {world_id}."
        msg = json.dumps(info)
        client.sendMessage(msg)
        print(f"Can't create new world. World is already exist with world id {world_id}.")
        return
    
    world_name = parsedMsg[Keys.WORLD_NAME.value]
    world_view_id = parsedMsg[Keys.WORLD_VIEW_ID.value]    
    world = World(world_id, world_name, world_view_id)
    
    worldsDict[world_id] = world
    
    info[Keys.INSTRUCTION.value] = Instructions.CREATE_WORLD.value
    info[Keys.WORLD_ID.value] = world_id
    info[Keys.WORLD_NAME.value] = world_name
    info[Keys.WORLD_VIEW_ID.value] = world_view_id
    msg = json.dumps(info)
    print(msg)
    encrypted_msg = encryptMsg(msg, client.publicKey)
    msg = json.dumps(encrypted_msg)
    client.sendMessage(msg)


def joinWorld(client : Client, parsedMsg : dict):
    print("Joining World")
    info : dict[str, str] = {}
    if not Keys.WORLD_ID.value in parsedMsg:
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = "No world id key is present in the msg."
        msg = json.dumps(info)
        client.sendMessage(msg)
        print("No world id key is present in the msg.")
        return

    if not Keys.AVATAR_ID.value in parsedMsg:
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = "No avatar id key is present in the msg."
        msg = json.dumps(info)
        client.sendMessage(msg)
        print("No avatar id key is present in the msg.")
        return

    world_id = parsedMsg[Keys.WORLD_ID.value]
    if not worldsDict.__contains__(world_id):
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = f"Can't join the world with id {world_id}. World with that id doesn't exists."
        msg = json.dumps(info)
        client.sendMessage(msg)
        return
    
    avatar_id = parsedMsg[Keys.AVATAR_ID.value]
    avatar = client.getAvatar(avatar_id)
    if avatar in worldsDict[world_id]:
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = f"{avatar.avatarName} has already joinned the world with id {world_id}."
        msg = json.dumps(info)
        client.sendMessage(msg)
        print(f"{avatar.avatarName} has already joinned the world with id {world_id}.")
        return
    
    worldsDict[world_id].addAvatar(avatar)
    
    info[Keys.INSTRUCTION.value] = Instructions.JOIN_WORLD.value
    
    info[Keys.WORLD_ID.value] = world_id
    info[Keys.WORLD_NAME.value] = worldsDict[world_id].worldName
    info[Keys.WORLD_VIEW_ID.value] = worldsDict[world_id].ViewId
    
    info[Keys.AVATAR_ID.value] = avatar_id
    info[Keys.AVATAR_NAME.value] = avatar.avatarName
    info[Keys.AVATAR_VIEW_ID.value] = avatar.viewId
    
    msg = json.dumps(info)
    print(msg)
    encrypted_msg = encryptMsg(msg, client.publicKey)
    msg = json.dumps(encrypted_msg)
    
    for ava in worldsDict[world_id].avatars:
        cl = getClientFromAvatarId(ava.avatarID)
        cl.sendMessage(msg)
    # client.sendMessage(msg)


def allWorlds(client : Client, parsedMsg : dict):
    print("Get All Worlds")
    info : dict[str, str] = {}
    
    info[Keys.INSTRUCTION.value] = Instructions.ALL_WORLDS.value   
    idx = 0
    for k in worldsDict:
        info[str(idx)] = worldsDict[k].worldId + "," + worldsDict[k].worldName + "," + worldsDict[k].ViewId
        idx += 1
    msg = json.dumps(info)
    print(msg)
    encrypted_msg = encryptMsg(msg, client.publicKey)
    msg = json.dumps(encrypted_msg)
    client.sendMessage(msg)


def clientAllAvatars(client : Client, parsedMsg : dict):
    print("Getting All Avatar of the client")
    info : dict[str, str] = {}
    info[Keys.INSTRUCTION.value] = Instructions.CLIENT_ALL_AVATARS.value
    for idx, avatar in enumerate(client.avatars):
        info[str(idx)] = avatar.avatarID + "," + avatar.avatarName + "," + avatar.viewId
    
    msg = json.dumps(info)
    print(msg)
    encrypted_msg = encryptMsg(msg, client.publicKey)
    msg = json.dumps(encrypted_msg)
    client.sendMessage(msg)


def worldAllAvatars(client : Client, parsedMsg : dict):
    print("Getting All Avatars of the World")
    info : dict[str, str] = {}

    if not Keys.WORLD_ID.value in parsedMsg:
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = "No world id key is present in the msg."
        msg = json.dumps(info)
        client.sendMessage(msg)
        print("No world id key is present in the msg.")
        return
    
    world_id = parsedMsg[Keys.WORLD_ID.value]
    if not worldsDict.__contains__(world_id):
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = f"Can't get avatars of the world with id {world_id}. World with that id doesn't exists."
        msg = json.dumps(info)
        client.sendMessage(msg)
        print(f"Can't get info of the world with id {world_id}. World with that id doesn't exists.")
        return

    world_avatars = worldsDict[world_id].avatars
    
    info[Keys.INSTRUCTION.value] = Instructions.WORLD_ALL_AVATARS.value
    for idx in range(len(world_avatars)):
        avatar = world_avatars[idx]
        info[str(idx)] = avatar.avatarID + "," + avatar.avatarName + "," + avatar.viewId
    
    msg = json.dumps(info)
    print(msg)
    encrypted_msg = encryptMsg(msg, client.publicKey)
    msg = json.dumps(encrypted_msg)
    client.sendMessage(msg)


def sendMessage(client : Client, parsedMsg : dict):
    print("Sending Message to Avatar")
    print(parsedMsg)
    info : dict[str, str] = {}
    
    if not Keys.WORLD_ID.value in parsedMsg:
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = "No world id key is present in the msg."
        msg = json.dumps(info)
        client.sendMessage(msg)
        print("No world id key is present in the msg.")
        return

    if not Keys.RECIEVER_ID.value in parsedMsg:
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = "No avatar id key is present in the msg."
        msg = json.dumps(info)
        client.sendMessage(msg)
        print("No avatar id key is present in the msg.")
        return

    if not Keys.MESSAGE.value in parsedMsg:
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = "No message key is present in the msg."
        msg = json.dumps(info)
        client.sendMessage(msg)
        print("No message key is present in the msg.")
        return

    world_id = parsedMsg[Keys.WORLD_ID.value]
    receiver_id = parsedMsg[Keys.RECIEVER_ID.value]
    
    is_receiver_found = False
    for avatar in worldsDict[world_id].avatars:
        if avatar.avatarID == receiver_id:
            is_receiver_found = True
            break
    
    if not is_receiver_found:
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = f"No avatar {receiver_id} is present in the world {world_id}."
        msg = json.dumps(info)
        client.sendMessage(msg)
        print(f"No avatar {receiver_id} is present in the world {world_id}.")
        return
    
    receiver : Client = getClientFromAvatarId(receiver_id)
    if receiver is None:
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = f"No receiver is found with avatar id {receiver_id}."
        msg = json.dumps(info)
        client.sendMessage(msg)
        print(f"No receiver is found with avatar id {receiver_id}.")
        return

    info[Keys.INSTRUCTION.value] = Instructions.SEND_MSG.value
    info[Keys.WORLD_ID.value] = world_id
    info[Keys.AVATAR_ID.value] = receiver_id
    info[Keys.MESSAGE.value] = parsedMsg[Keys.MESSAGE.value]
    msg = json.dumps(info)
    print(msg)
    encrypted_msg = encryptMsg(msg, client.publicKey)
    msg = json.dumps(encrypted_msg)
    client.sendMessage(msg)


def parseMessage(client : Client, msg : str):
    parsedMsg : dict[str, str] = json.loads(msg)
    if not Keys.MSG_TYPE.value in parsedMsg:
        print("No msg type is given with the msg...")
        DecodeInstruction(client, parsedMsg)
    else:
        msg_type = parsedMsg[Keys.MSG_TYPE.value]
        if msg_type == MessageType.ENCRYPTED_TEXT.value:
            decryptMsg(client, parsedMsg)
        else:
            DecodeInstruction(client, parsedMsg)
    

def handleClient(client_socket : socket):
    h, p = client_socket.getpeername()
    client = getClientFromAddress(h, p)
    
    info : dict[str, str] = {}
    if client != None:
        info[Keys.INSTRUCTION.value] = Instructions.ERROR.value
        info[Keys.MESSAGE.value] = f"You {client_socket.getpeername()} are already joinned..."
        print(f"Already joinned. Number of clients joinned {len(clients)}")
    else:
        client = Client(client_socket)
        info[Keys.INSTRUCTION.value] = Instructions.SERVER_KEY.value
        info[Keys.PUBLIC_KEY.value] = bytes.hex(public_key)
        info[Keys.MESSAGE.value] = f"You {client_socket.getpeername()} join newly..."
        clients.add(client)
        print(f"After adding new client, Number of clients {len(clients)}.")

    try:
        print(info)
        msg = json.dumps(info)
        client.sendMessage(msg)
        while True:
            try:
                msg = client.receiveMessage(DATA_BUFFER_SIZE)
                if msg == None or msg == '':
                    print("No message is sent")
                    client.close()
                    break
                print(msg)
                parseMessage(client, msg)
            except KeyboardInterrupt:
                client.close()
                break
            # except Exception as e:
            #     print(e)
            #     print("Some error in receiving data -> " + str(e))
            #     client.close()
            #     break
    except KeyboardInterrupt:
        client.close()
    # except Exception as e:
    #     print("Some error in sending data -> " + str(e))
    #     client.close()
        
    
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
from socket import socket, AF_INET, SOCK_STREAM
from threading import Thread
from utilities import Instructions, Keys
import json

client = socket(AF_INET, SOCK_STREAM)

host = "127.0.0.1"
port = 12000
try:
    client.connect((host, port))
except:
    print("Can't connect to the server!")
    exit(0)

print(client.getsockname())

encoded_msg = client.recv(1024)
print(encoded_msg.decode())

encoded_msg = client.recv(1024)
print(encoded_msg.decode())

is_running = True

info = {}
info[Keys.INSTRUCTION.value] = Instructions.SENT_KEY.value
info[Keys.PUBLIC_KEY.value] = "waidiwhauihduiwhad"

client.send(json.dumps(info).encode())

def ReceiveThread():
    while True:
        try:
            encoded_msg = client.recv(1024)
            msg = encoded_msg.decode()
            if msg != None and msg != '':
                print(msg)
            else:
                # print("Null message is received...")
                continue
        except:
            print("Socket is closed")
            global is_running
            is_running = False
            client.close()
            break

receiveThread = Thread(target=ReceiveThread)
receiveThread.daemon = True
receiveThread.start()

while is_running:
    try:
        ch = input("Enter CA (create avatar) or CW (create world) or JW (join world) or SM (send msg) or ALLW (all worlds) or WA (world all avatars) or AI (avatar info)")
        ch = ch.lower()
        info : dict = {}
        if ch == 'ca' or ch == 'create avatar':
            avatar_name = input("Enter avatar name : ")
            info[Keys.INSTRUCTION.value] = Instructions.CREATE_AVATAR.value
            info[Keys.AVATAR_NAME.value] = avatar_name
            info[Keys.AVATAR_VIEW_ID.value] = "view_id"
            msg = json.dumps(info)
            client.send(msg.encode())
        elif ch == 'cw' or ch == 'create world':
            world_name = input("Enter world name to create : ")
            info[Keys.INSTRUCTION.value] = Instructions.CREATE_WORLD.value
            info[Keys.WORLD_NAME.value] = world_name
            info[Keys.WORLD_VIEW_ID.value] = "view_id"
            msg = json.dumps(info)
            client.send(msg.encode())
        elif ch == "jw" or ch == 'join world':
            world_id = input("Enter world id to join : ")
            avatar_id = input("Enter avatar id to join: ")
            info[Keys.INSTRUCTION.value] = Instructions.JOIN_WORLD.value
            info[Keys.WORLD_ID.value] = world_id
            info[Keys.AVATAR_ID.value] = avatar_id
            msg = json.dumps(info)
            client.send(msg.encode())
        elif ch == 'allw' or ch == 'all worlds':
            info[Keys.INSTRUCTION.value] = Instructions.ALL_WORLDS.value
            msg = json.dumps(info)
            client.send(msg.encode())
        elif ch == 'wa' or ch == 'world all avatars':
            world_id = input("Enter world id to get info : ")
            info[Keys.INSTRUCTION.value] = Instructions.WORLD_ALL_AVATARS.value
            info[Keys.WORLD_ID.value] = world_id
            msg = json.dumps(info)
            client.send(msg.encode())
        elif ch == 'ai' or ch == 'avatar info':
            info[Keys.INSTRUCTION.value] = Instructions.CLIENT_ALL_AVATARS.value
            msg = json.dumps(info)
            client.send(msg.encode())
        elif ch == 'sm' or ch == 'send msg':
            world_id = input("Enter avatar id to send : ")
            avatar_id = input("Enter avatar id to send : ")
            msg = input("Enter msg to send : ")
            info[Keys.INSTRUCTION.value] = Instructions.SEND_MSG.value
            info[Keys.WORLD_ID.value] = world_id
            info[Keys.AVATAR_ID.value] = avatar_id
            info[Keys.MESSAGE.value] = msg
            enc_msg = json.dumps(info)
            print(enc_msg)
            client.send(enc_msg.encode())
        elif ch == 'exit':
            client.close()
            break
    except KeyboardInterrupt:
        client.close()
        break
    except:
        client.close()
        break
from socket import socket, AF_INET, SOCK_STREAM
from threading import Thread

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
client.send("new client".encode())

is_running = True

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
        ch = input("Enter CA (create avatar) or CW (create world) or JW (join world) or SM (send msg) or AW (all worlds) or WI (world info) or AI (avatar info)")
        ch = ch.lower()
        if ch == 'ca' or ch == 'create avatar':
            msg = "create_avatar\n"
            avatar_name = input("Enter avatar name : ")
            msg += avatar_name
            client.send(msg.encode())
        elif ch == 'cw' or ch == 'create world':
            world_name = input("Enter world name to create : ")
            msg = "create_world\n" + world_name
            client.send(msg.encode())
        elif ch == "jw" or ch == 'join world':
            world_id = input("Enter world id to join : ")
            msg = "join_world\n" + world_id
            client.send(msg.encode())
        elif ch == 'sm' or ch == 'send msg':
            h = '127.0.0.1'
            p = input("Enter port : ")
            msg = input("Enter msg to send : ")
            enc_msg = "send_msg\n" + h + " " + str(p) + "\n"
            enc_msg += msg
            client.send(enc_msg.encode())
        elif ch == 'aw' or ch == 'all worlds':
            client.send("all_worlds\n".encode())
        elif ch == 'wi' or ch == 'world info':
            world_id = input("Enter world id to get info : ")
            client.send(f"world_info\n{world_id}".encode())
        elif ch == 'ai' or ch == 'avatar info':
            client.send("avatar_info\n".encode())
        elif ch == 'exit':
            client.close()
            break
    except KeyboardInterrupt:
        client.close()
    except:
        client.close()
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
        print("Thread is running...")
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
        ch = input("Enter C (create) or J (join) or S (send) or All (all worlds) or World (world info)")
        ch = ch.lower()
        if ch == 'c' or ch == 'create':
            world_id = input("Enter world id to create : ")  
            msg = "create\n" + world_id
            client.send(msg.encode())
        elif ch == "j" or ch == 'join':
            world_id = input("Enter world id to join : ")
            msg = "join\n" + world_id
            client.send(msg.encode())
        elif ch == 's' or ch == 'send':
            h = '127.0.0.1'
            p = input("Enter port : ")
            msg = input("Enter msg to send : ")
            enc_msg = "send\n" + h + " " + str(p) + "\n"
            enc_msg += msg
            client.send(enc_msg.encode())
        elif ch == 'all' or ch == 'all worlds':
            client.send("all_worlds\n".encode())
        elif ch == 'world' or ch == 'world info':
            world_id = input("Enter world id to get info : ")
            client.send(f"world_info\n{world_id}".encode())
    except KeyboardInterrupt:
        client.close()
    except:
        client.close()
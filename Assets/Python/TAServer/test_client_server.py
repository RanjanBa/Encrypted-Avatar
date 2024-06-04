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
client.send("public key".encode())

def ReceiveThread():
    while True:
        try:
            encoded_msg = client.recv(1024)
            print(encoded_msg.decode())
        except:
            client.close()
            break

while True:
    try:
        receiveThread = Thread(target=ReceiveThread)
        receiveThread.daemon = True
        receiveThread.start()
        h = input("Enter host name where to send : ")
        p = input("Enter port number where to send : ")
        msg = input("Enter msg to send : ")
        enc_msg = h + " " + p + "\n"
        enc_msg += msg
        client.send(enc_msg.encode())
    except KeyboardInterrupt:
        client.close()
        break
    except:
        client.close()
        break
import sys

sys.path.append('./Kyber')

from socket import socket, AF_INET, SOCK_STREAM
from threading import Thread
from client import Client
from kyber import Kyber1024

host = "127.0.0.1"
port = 12000

main_server = socket(AF_INET, SOCK_STREAM)
main_server.bind((host, port))
main_server.listen(5)

clients : list[Client] = {}

pk, sk = Kyber1024.keygen()

def getClient(address, port):
    for client in clients:
        h, p = client.socket.getpeername()
        if h == address and p == port:
            return client
        
    return None

def handleClient(client_socket : socket):
    try:
        client_socket.send(f"Hello Client!\npublic_key: {bytes.hex(pk)}".encode())
        msg = client_socket.recv(1024)
        print(msg.decode())
        clients.append(Client(client_socket))
    except:
        print("Some error in receiving or sending data...")
        client_socket.close()
        return

    while True:
        try:
            encoded_msg = client_socket.recv(1024)
            msg = encoded_msg.decode()
            print(msg)
            sentences = msg.split('\n')
            h, p = sentences[0].split()
            receiver : socket = getClient(h, int(p)).socket
            if receiver != None:
                print(receiver)
                msg = sentences[1]
                receiver.send(msg.encode())
            else:
                print("Can't find receiver")
                client_socket.send("Can't find receiver client".encode())
        except KeyboardInterrupt:
            client_socket.close()
            break


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
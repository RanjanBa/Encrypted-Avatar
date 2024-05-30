from socket import socket, AF_INET, SOCK_STREAM
from threading import Thread

host = "127.0.0.1"
port = 12000


server = socket(AF_INET, SOCK_STREAM)
server.bind((host, port))
server.listen(5)

clients : dict = {}

def handleClient(client_socket : socket):
    client_socket.send("Hello Client!".encode())
    print(client_socket)

    msg = client_socket.recv(1024)
    print(msg.decode())
    h, p = client_socket.getpeername()
    clients[(h, p)] = client_socket

    while True:
        try:
            encoded_msg = client_socket.recv(1024)
            msg = encoded_msg.decode()
            print(msg)
            sentences = msg.split('\n')
            h, p = sentences[0].split()
            client : socket = clients[(h, int(p))]
            print(client)
            msg = sentences[1]
            if client != None:
                client.send(msg.encode())
        except KeyboardInterrupt:
            break


def startServer():
    while True:
        try:
            print("Server is listening and waiting for new client...")
            client_socket, address = server.accept()
            print(f"New client with address {address} is established.")
            thread = Thread(target=handleClient, args=(client_socket,))
            thread.daemon = True
            thread.start()
        except KeyboardInterrupt:
            print("Keyboard is interrupted.")
            server.close()
            break
        except:
            print("Unknown error!")
            server.close()
            break

if __name__ == "__main__":
    startServer()
from socket import socket, AF_INET, SOCK_STREAM
from threading import Thread

host = "127.0.0.1"
port = 12000


server = socket(AF_INET, SOCK_STREAM)
server.bind((host, port))
server.listen(5)

def handleClient(client_socket : socket):
    print(client_socket)

    client_socket.send("Hello from server!".encode())
    while True:
        try:
            encoded_msg = client_socket.recv(1024)
            print(encoded_msg.decode('utf-8'))
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
from socket import socket, AF_INET, SOCK_STREAM
from threading import Thread

client = socket(AF_INET, SOCK_STREAM)

host = "127.0.0.1"
port = 12000
client.connect((host, port))

while True:
    encoded_msg = client.recv(1024)
    print(encoded_msg.decode())
    msg = input("Enter msg to send : ")
    client.send(msg.encode())
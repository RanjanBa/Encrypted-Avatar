import socket
import threading

host = "127.0.0.1"
port = 9000

alias = input('Choose an alias >>> ')

client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
client.connect((host, port))

def clientReceive():
    while True:
        try:
            msg = client.recv(1024).decode('utf-8')
            if msg == "alias?":
                client.send(alias.encode('utf-8'))
            else:
                print(msg)
        except:
            print("Error!")
            client.close()
            break


def clientSend():
    while True:
        msg = f'{alias} : {input("")}'
        try:
            client.send(msg.encode('utf-8'))
        except:
            print("Error!")
            client.close()
            break


receive_thread = threading.Thread(target=clientReceive)
receive_thread.start()

send_thread = threading.Thread(target=clientSend)
send_thread.start()
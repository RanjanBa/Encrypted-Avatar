import sys

sys.path.append('./')
sys.path.append('./Kyber')

from socket import socket, AF_INET, SOCK_STREAM
from threading import Thread
from utilities import Instructions, Keys
import json

DATA_BUFFER_SIZE = 10240

host = "127.0.0.1"
port = 9000

client = socket(AF_INET, SOCK_STREAM)

try:
    client.connect((host, port))
except:
    print("Can't connect to the server!")
    exit(0)

print(client.getsockname())

encrypted_msg = client.recv(1024)
print(encrypted_msg.decode())

is_running = True
public_key = None
private_key = None
encrypted_msg = None

def parseMessage(msg : str):
    parsedMsg = json.loads(msg)
    if not Keys.INSTRUCTION.value in parsedMsg:
        print("No instruction is given with the msg...")
        return

    msg_code = parsedMsg[Keys.INSTRUCTION.value]
    if msg_code == Instructions.GENERATE_KEY.value:
        global public_key
        global private_key
        public_key = parsedMsg[Keys.KYBER_PUBLIC_KEY.value]
        private_key = parsedMsg[Keys.KYBER_PRIVATE_KEY.value]
    elif msg_code == Instructions.ENCRYPT_MSG.value:
        global encrypted_msg
        encrypted_msg = {}
        encrypted_msg[Keys.ENCAPSULATED_KEY.value] = parsedMsg[Keys.ENCAPSULATED_KEY.value]
        encrypted_msg[Keys.TAG.value] = parsedMsg[Keys.TAG.value]
        encrypted_msg[Keys.CIPHER_TEXT.value] = parsedMsg[Keys.CIPHER_TEXT.value]
        encrypted_msg[Keys.NONCE.value] = parsedMsg[Keys.NONCE.value]
    elif msg_code == Instructions.DECRYPT_MSG.value:
        print(parsedMsg[Keys.MESSAGE.value])

def ReceiveThread():
    while True:
        try:
            encoded_msg = client.recv(DATA_BUFFER_SIZE)
            msg = encoded_msg.decode()
            if msg != None and msg != '':
                print(msg)
                parseMessage(msg)
            else:
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
        ch = input("Enter G (generate key) or E (encrypt msg) or D (decrypt msg)")
        ch = ch.lower()
        info : dict = {}
        if ch == 'g' or ch == 'generate key':
            info[Keys.INSTRUCTION.value] = Instructions.GENERATE_KEY.value
            msg = json.dumps(info)
            client.send(msg.encode())
        elif ch == 'e' or ch == 'encrypt msg':
            if public_key == None:
                print("First generate public key and private key.")
                continue
            
            msg = input("Enter msg to encrypt : ")
            info[Keys.INSTRUCTION.value] = Instructions.ENCRYPT_MSG.value
            info[Keys.KYBER_PUBLIC_KEY.value] = public_key
            info[Keys.MESSAGE.value] = msg
            client.send(json.dumps(info).encode())
        elif ch == "d" or ch == 'decrypt msg':
            if encrypted_msg == None:
                print("First encrypt the message.")
                continue
            encrypted_msg[Keys.INSTRUCTION.value] = Instructions.DECRYPT_MSG.value
            encrypted_msg[Keys.KYBER_PRIVATE_KEY.value] = private_key
            msg = json.dumps(encrypted_msg)
            client.send(msg.encode())
        elif ch == 'exit':
            client.close()
            break
    except KeyboardInterrupt:
        client.close()
        break
    except:
        client.close()
        break
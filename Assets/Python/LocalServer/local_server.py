import sys

sys.path.append('./')
sys.path.append('./Kyber')

import socket
import threading
import json
from typing import List
from client import Client
from utilities import LocalInstructions, Keys

# import rsa_encrypt_decrypt
import kyber_encrypt_decrypt

DATA_BUFFER_SIZE = 10240

host = "127.0.0.1"
port = 9000

server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind((host, port))
server.listen(5)

clients : List[Client] = []


def handleClients(client : Client):
    info = {}
    info[Keys.MESSAGE.value] = f"You are connected to the local server {server.getsockname()} successfully.";
    client.sendMessage(json.dumps(info))
    clients.append(client)
    print(f"Number of Local clients {len(clients)}")

    while True:
        try:
            msg = client.receiveMessage(DATA_BUFFER_SIZE)
            if msg == None or msg == "":
                # print("No message is sent")
                break

            # print(f"Msg received from {client.socket.getpeername()} : {msg}")
            parseMessage(client, msg)
        except KeyboardInterrupt:
            # print("Keyboard Interruption...")
            break
        except Exception as e:
            # print(f"Some error occurs... {e}")
            break
        
    if(client in clients):
        clients.remove(client)
    client.close()
    print(f"Client left the local server!")
    print(f"Number of clients left on local server -> {len(clients)}")


def parseMessage(client : Client, msg : str):
    parsedMsg = json.loads(msg)
    if not Keys.INSTRUCTION.value in parsedMsg:
        print("No instruction is given with the msg...")
        return

    msg_code = parsedMsg[Keys.INSTRUCTION.value]

    if msg_code == LocalInstructions.GENERATE_KEY.value:
        print("Generating and Sending Public and Secret Key...")
        # pk, sk = rsa_encrypt_decrypt.getKey()
        pk, sk = kyber_encrypt_decrypt.getKey()
        
        public_key = bytes.hex(pk)
        secret_key = bytes.hex(sk)

        info : dict[str, str] = {}
        info[Keys.INSTRUCTION.value] = LocalInstructions.GENERATE_KEY.value
        info[Keys.PUBLIC_KEY.value] = public_key
        info[Keys.PRIVATE_KEY.value] = secret_key
        msg = json.dumps(info)
        client.sendMessage(msg)
        print("Public and Secret key Sent...")
    elif msg_code == LocalInstructions.ENCRYPT_MSG.value:
        public_key = parsedMsg[Keys.PUBLIC_KEY.value]
        if public_key == "":
            print("Invalid public key.")
            return

        print("Encrypting Msg...")
        msg = parsedMsg[Keys.MESSAGE.value]
        public_key = bytes.fromhex(public_key)
        encapsulated_key, cipher_text, tag, nonce = kyber_encrypt_decrypt.encrypt(msg, public_key)  
        info : dict[str, str] = {}
        info[Keys.INSTRUCTION.value] = LocalInstructions.ENCRYPT_MSG.value
        info[Keys.ENCAPSULATED_KEY.value] = bytes.hex(encapsulated_key)
        info[Keys.CIPHER_TEXT.value] = bytes.hex(cipher_text)
        info[Keys.TAG.value] = bytes.hex(tag)
        info[Keys.NONCE.value] = bytes.hex(nonce)
        response = json.dumps(info)
        client.sendMessage(response)
        print("Encrypted Msg Sent...")
    elif msg_code == LocalInstructions.DECRYPT_MSG.value:
        print("Decrypting Msg...")
        private_key = parsedMsg[Keys.PRIVATE_KEY.value]
        encapsulated_key = parsedMsg[Keys.ENCAPSULATED_KEY.value]
        cipher_text = parsedMsg[Keys.CIPHER_TEXT.value]
        tag = parsedMsg[Keys.TAG.value]
        nonce = parsedMsg[Keys.NONCE.value]

        if private_key == "" or encapsulated_key == "" or tag == "" or cipher_text == "" or nonce == "":
            print("Invalid private key or enc_session_key or tag or ciphertext or nonce!")
            return

        private_key = bytes.fromhex(private_key)
        encapsulated_key = bytes.fromhex(encapsulated_key)
        tag = bytes.fromhex(tag)
        cipher_text = bytes.fromhex(cipher_text)
        nonce = bytes.fromhex(nonce)

        # decrypted_msg = rsa_encrypt_decrypt.decrypt(private_key, enc_session_key, cipher_text, tag, nonce)
        decrypted_msg = kyber_encrypt_decrypt.decrypt(private_key, encapsulated_key, cipher_text, tag, nonce)
        info : dict[str, str] = {}
        info[Keys.INSTRUCTION.value] = LocalInstructions.DECRYPT_MSG.value
        info[Keys.MESSAGE.value] = decrypted_msg
        client.sendMessage(json.dumps(info))
        print("Decrypted Msg Sent...")
    else:
        print(f"Message is sent without proper instruction -> {msg_code}")
        pass


def startServer():
    while True:
        try:
            print("Local Server is running and waiting for connection ...")
            client_socket, address = server.accept()
            print(f"New Client {address} is established with local Server.")
            
            client = Client(client_socket)
            
            thread = threading.Thread(target=handleClients, args=(client,))
            thread.daemon = True
            thread.start()
        except KeyboardInterrupt:
            # print("Keyboard Interruption...")
            server.close()
            break
        except:
            # print("Error start server!")
            s = input("Enter 'Yes' for continuation... : ")

            if s.lower() == 'yes':
                continue
            else:
                server.close()
                break


if __name__ == "__main__":
    startServer()
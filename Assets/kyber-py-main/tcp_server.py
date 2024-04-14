import socket
import threading

import rsa_encrypt_decrypt

host = "127.0.0.1"
port = 9000
data_size = 10240

server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind((host, port))
server.listen(5)

class Client:
    def __init__(self, socket : socket.socket) -> None:
        self.__socket = socket
        self.__alias = None
    
    @property
    def socket(self):
        return self.__socket
    
    @property
    def alias(self):
        return self.__alias
    
    @alias.setter
    def alias(self, name):
        self.__alias = name
    
    def sendMessage(self, msg : str) -> None:
        self.__socket.send(msg.encode("utf-8"))
        # try:
        #     self.__socket.send(msg.encode("utf-8"))
        # except ConnectionResetError as err:
        #     print(str(err))
        #     raise Exception()
        
    def recvMessage(self) -> str:
        msg = self.socket.recv(data_size)
        return msg.decode("utf-8")
        # try: 
        #     msg = self.socket.recv(data_size)
        #     return msg.decode("utf-8")
        # except:
        #     print("Error in receiving!")
        #     raise Exception()

    def close(self) -> None:
        try:
            self.socket.close()
        except:
            print("Error in socket closing!")
            raise Exception()

clients : list[Client] = []


def broadcast(msg : str):
    for client in clients:
        client.sendMessage(msg)


def handleClients(client : Client):
    client.sendMessage(f"You are connected to the server {server.getsockname()} successfully")
    alias = client.recvMessage()
    client.alias = alias
    print(f"The alias of the new client {client.socket.getpeername()} is {alias}")
    broadcast(f"{alias} is connected to the server!")
    clients.append(client)
    print(f"Number of clients {len(clients)}")
    while True:
        try:
            msg = client.recvMessage()
            if msg != None and msg != "":
                print(msg)
            else:
                continue

            parseMessage(client, msg)
        except:
            if(client in clients):
                clients.remove(client)
            client.close()
            print(f"{client.alias} left the server!")
            print(f"Number of clients {len(clients)}")
            break


def parseMessage(client : Client, msg : str):
    sentences = msg.split('\n')
    
    msg_code = sentences[0].strip()
    if msg_code == "get_key":
        print("Sending...")
        pk, sk = rsa_encrypt_decrypt.getKey()
        
        public_key = bytes.hex(pk)
        secret_key = bytes.hex(sk)

        client.sendMessage(f"public_key : {public_key}\nprivate_key : {secret_key}")
        print("Sending key Completed...")
    elif msg_code == "encrypt_msg":
        print("Encrypting Msg...")
        msg = ""
        public_key = ""
        for i in range(1, len(sentences)):
            sentence = sentences[i].strip()
            if len(sentence) == 0:
                continue
        
            words = sentence.split(":")
            for i in range(len(words)):
                words[i] = words[i].strip()
            
            if words[0] == "msg":
                msg = words[1]
            elif words[0] == "public_key":
                public_key = words[1]
            else:
                print("Invalid msg!")
        
        if msg == "" or public_key == "":
            print("Invalid msg or public key!")
            return

        public_key = bytes.fromhex(public_key)
        enc_session_key, tag, ciphertext, nonce = rsa_encrypt_decrypt.encrypt(msg, public_key)
        
        response = f"enc_session_key:{bytes.hex(enc_session_key)}\n tag:{bytes.hex(tag)}\n ciphertext:{bytes.hex(ciphertext)}\n nonce:{bytes.hex(nonce)}"
        client.sendMessage(response)
        print("Encrypted Msg Sent...")
    elif msg_code == "decrypt_msg":
        print("Decrypting Msg...")
        
        private_key = ""
        enc_session_key = ""
        tag = ""
        ciphertext = ""
        nonce = ""

        for i in range(1, len(sentences)):
            sentence = sentences[i].strip()
            if len(sentence) == 0:
                continue

            words = sentence.split(":")
            for i in range(len(words)):
                words[i] = words[i].strip()
            
            if words[0] == "private_key":
                private_key = words[1]
            elif words[0] == "enc_session_key":
                enc_session_key = words[1]
            elif words[0] == "tag":
                tag = words[1]
            elif words[0] == "ciphertext":
                ciphertext = words[1]
            elif words[0] == "nonce":
                nonce = words[1]
            else:
                print("Invalid msg!")

        if private_key == "" or enc_session_key == "" or tag == "" or ciphertext == "" or nonce == "":
            print("Invalid private key or enc_session_key or tag or ciphertext or nonce!")
            return

        private_key = bytes.fromhex(private_key)
        enc_session_key = bytes.fromhex(enc_session_key)
        tag = bytes.fromhex(tag)
        ciphertext = bytes.fromhex(ciphertext)
        nonce = bytes.fromhex(nonce)

        decrypt_msg = rsa_encrypt_decrypt.decrypt(private_key, enc_session_key, tag, ciphertext, nonce)
        client.sendMessage(decrypt_msg)
        print("Decrypting Msg Sent...")
    else:
        # client.sendMessage(f"Error in the request!")
        print("Error in the request!")


def startServer():
    while True:
        try:
            print(f"Number of clients : {len(clients)}")
            print("Server is running and waiting for connection ...")
            clientsocket, address = server.accept()
            print(f"New Client establishes connection with {address}")
            
            client = Client(clientsocket)
            
            thread = threading.Thread(target=handleClients, args=(client,))
            thread.start()
        except:
            print("Error start server!")


if __name__ == "__main__":
    startServer()
import socket
import threading
from typing import List

# import rsa_encrypt_decrypt
import kyber_encrypt_decrypt

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
        self.__public_key = None
    
    @property
    def socket(self):
        return self.__socket
    
    @property
    def alias(self):
        return self.__alias
    
    @property
    def public_key(self):
        return self.__public_key
    
    @alias.setter
    def alias(self, name):
        self.__alias = name
    
    @public_key.setter
    def public_key(self, key):
        self.__public_key = key

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


clients : List[Client] = []


def broadcast(msg : str):
    for client in clients:
        client.sendMessage(msg)


def handleClients(client : Client):
    client.sendMessage(f"You are connected to the server {server.getsockname()} successfully")
    alias = client.recvMessage()
    alias = alias.strip('\n')
    client.alias = alias
    
    print(f"The alias of the new client {client.socket.getpeername()} is {alias}")
    broadcast(f"{alias} is connected to the server!\n")
    clients.append(client)
    print(f"Number of clients {len(clients)}")
    
    print("Sending...")
    # pk, sk = rsa_encrypt_decrypt.getKey()
    pk, sk = kyber_encrypt_decrypt.getKey()
    
    public_key = bytes.hex(pk)
    secret_key = bytes.hex(sk)

    client.sendMessage(f"generate_key\npublic_key : {public_key}\nprivate_key : {secret_key}")
    print("Sending key Completed...")
    client.public_key = public_key

    while True:
        try:
            msg = client.recvMessage()
            if msg != None and msg != "":
                print(msg)
            else:
                continue
            
            print(f"Msg received from {client.alias} : {msg}")
            parseMessage(client, msg)
        except KeyboardInterrupt:
            print("Keyboard Interruption...")
            client.close()
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

    if msg_code == "send_msg":
        receiver_name = sentences[1].strip()
        new_msg = ""
        for i in range(2, len(sentences)):
            new_msg += sentences[i]
            new_msg += '\n'
        
        receiver_client = None
        names = []
        for client in clients:
            names.append(client.alias)
        print("Clients' names are : ", end = "")
        print(names)
        for client in clients:
            
            if client.alias == receiver_name:
                receiver_client = client
                break
        
        if receiver_client == None:
            print(f"No client is found with name '{receiver_name}'")
            return
        
        public_key = bytes.fromhex(receiver_client.public_key)
        # enc_session_key, tag, ciphertext, nonce = rsa_encrypt_decrypt.encrypt(new_msg, public_key)
        enc_session_key, tag, ciphertext, nonce = kyber_encrypt_decrypt.encrypt(new_msg, public_key)
        
        response = f"encrypt_msg\nenc_session_key:{bytes.hex(enc_session_key)}\n tag:{bytes.hex(tag)}\n ciphertext:{bytes.hex(ciphertext)}\n nonce:{bytes.hex(nonce)}"
        receiver_client.sendMessage(response)
        print(f"Msg Sent to Receiver {receiver_client.alias}...")
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
        # enc_session_key, tag, ciphertext, nonce = rsa_encrypt_decrypt.encrypt(msg, public_key)
        enc_session_key, tag, ciphertext, nonce = kyber_encrypt_decrypt.encrypt(msg, public_key)
        
        
        response = f"encrypt_msg\nenc_session_key:{bytes.hex(enc_session_key)}\ntag:{bytes.hex(tag)}\nciphertext:{bytes.hex(ciphertext)}\nnonce:{bytes.hex(nonce)}"
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

        # decrypted_msg = rsa_encrypt_decrypt.decrypt(private_key, enc_session_key, tag, ciphertext, nonce)
        decrypted_msg = kyber_encrypt_decrypt.decrypt(private_key, enc_session_key, tag, ciphertext, nonce)
        client.sendMessage(f"decrypt_msg\n{decrypted_msg}")
        print("Decrypted Msg Sent...")
    else:
        # client.sendMessage(f"Error in the request!")
        print("Error in the request!")


def startServer():
    try:
        while True:
            try:
                print(f"Number of clients : {len(clients)}")
                print("Server is running and waiting for connection ...")
                client_socket, address = server.accept()
                print(f"New Client establishes connection with {address}")
                
                client = Client(client_socket)
                
                thread = threading.Thread(target=handleClients, args=(client,))
                thread.start()
            except:
                print("Error start server!")
                s = input("Enter 'Yes' for continuation... : ")

                if s.lower() == 'yes':
                    continue
                else:
                    server.close()
                    break
    except KeyboardInterrupt:
        print("Keyboard Interruption...")
    finally:
        server.close()


if __name__ == "__main__":
    startServer()
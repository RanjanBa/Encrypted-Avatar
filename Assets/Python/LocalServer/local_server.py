import sys

sys.path.append('./')

import socket
import threading
import json
from typing import List
from client import Client
from utilities import LocalInstructions, Keys, VerificationStatus

import kyber_encrypt_decrypt
import dilithium_sign_verify

DATA_BUFFER_SIZE = 40560

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
                print("No message is sent")
                break

            # print(f"Msg received from {client.socket.getpeername()} : {msg}")
            parseMessage(client, msg)
        except KeyboardInterrupt:
            print("Keyboard Interruption...")
            break
        except Exception as e:
            print(f"Some error occurs... {e}")
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
        kyber_pk, kyber_sk = kyber_encrypt_decrypt.getKeys()
        
        kyber_public_key = bytes.hex(kyber_pk)
        kyber_secret_key = bytes.hex(kyber_sk)

        dilithium_pk, dilithium_sk = dilithium_sign_verify.getKeys()
        
        dilithium_public_key = bytes.hex(dilithium_pk)
        dilithium_secret_key = bytes.hex(dilithium_sk)

        info : dict[str, str] = {}
        info[Keys.INSTRUCTION.value] = LocalInstructions.GENERATE_KEY.value
        info[Keys.KYBER_PUBLIC_KEY.value] = kyber_public_key
        info[Keys.KYBER_PRIVATE_KEY.value] = kyber_secret_key
        info[Keys.DILITHIUM_PUBLIC_KEY.value] = dilithium_public_key
        info[Keys.DILITHIUM_PRIVATE_KEY.value] = dilithium_secret_key
        msg = json.dumps(info)
        client.sendMessage(msg)
        print("Public and Secret key Sent...")
    elif msg_code == LocalInstructions.ENCRYPT_MSG.value:
        public_key = parsedMsg[Keys.KYBER_PUBLIC_KEY.value]
        if public_key == "":
            print("Public key is empty...")
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
        private_key = parsedMsg[Keys.KYBER_PRIVATE_KEY.value]
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
        decrypted_msg = decrypted_msg.decode('utf-8')
        info : dict[str, str] = {}
        info[Keys.INSTRUCTION.value] = LocalInstructions.DECRYPT_MSG.value
        info[Keys.MESSAGE.value] = decrypted_msg
        client.sendMessage(json.dumps(info))
        print("Decrypted Msg Sent...")
    elif msg_code == LocalInstructions.SIGN_MSG.value:
        private_key = parsedMsg[Keys.DILITHIUM_PRIVATE_KEY.value]
        if private_key == "":
            print("Private key is empty...")
            return
        print("Signing Msg...")
        msg = parsedMsg[Keys.MESSAGE.value]
        private_key = bytes.fromhex(private_key)
        sign = dilithium_sign_verify.signature(msg, private_key)
        info : dict[str, str] = {}
        info[Keys.INSTRUCTION.value] = LocalInstructions.SIGN_MSG.value
        info[Keys.SIGNATURE.value] = bytes.hex(sign)
        info[Keys.MESSAGE.value] = msg
        client.sendMessage(json.dumps(info))
        print("Signature Msg Sent...")
    elif msg_code == LocalInstructions.VERIFY_MSG.value:
        public_key = parsedMsg[Keys.DILITHIUM_PUBLIC_KEY.value]
        if public_key == "":
            print("Public key is empty...")
            return
        print("Verifying msg")
        sign = parsedMsg[Keys.SIGNATURE.value]
        msg = parsedMsg[Keys.MESSAGE.value]
        public_key = bytes.fromhex(public_key)
        is_verfied = dilithium_sign_verify.verify(public_key, bytes.fromhex(sign), msg)
        info : dict[str, str] = {}
        info[Keys.INSTRUCTION.value] = LocalInstructions.VERIFY_MSG.value
        if is_verfied:
            info[Keys.VERIFICATION_STATUS.value] = VerificationStatus.VERIFIED.value
        else:
            info[Keys.VERIFICATION_STATUS.value] = VerificationStatus.UNVERIFIED.value
        info[Keys.MESSAGE.value] = msg
        client.sendMessage(json.dumps(info))
        print("Verification completed...")
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
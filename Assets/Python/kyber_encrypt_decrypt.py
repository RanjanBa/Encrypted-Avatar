import sys

# sys.path.append('./Kyber')

from Crypto.Cipher import AES
from Kyber.kyber import Kyber1024
# from Crypto.Hash import SHA256


def getKeys():
    public_key, private_key = Kyber1024.keygen()
    return (public_key, private_key)


def encrypt(msg : str, public_key : bytes): 
    shared_key, encapsulated_text = Kyber1024.encaps(public_key)
    # Encrypt the data with the AES session key
    # sha_object = SHA256.new(shared_key)
    # shared_key = sha_object.digest()
    cipher_aes = AES.new(shared_key, AES.MODE_EAX)

    encoded_msg = msg.encode("utf-8")
    cipher_text, tag = cipher_aes.encrypt_and_digest(encoded_msg)
    return (encapsulated_text, cipher_text, tag, cipher_aes.nonce)


def decrypt(private_key : bytes, encapsulated_text : bytes, cipher_text : bytes, tag : bytes, nonce : bytes):
    shared_key = Kyber1024.decaps(private_key, encapsulated_text)
    # Decrypt the data with the AES session key
    # sha_object = SHA256.new(shared_key)
    # shared_key = sha_object.digest()

    cipher_aes = AES.new(shared_key, AES.MODE_EAX, nonce)
    data = cipher_aes.decrypt_and_verify(cipher_text, tag)
    return data


def main():
    msg = "Hello world. I am Ranjan. This is Cryptography test."
    if len(sys.argv) > 1:
        msg = ""
        for idx, m in enumerate(sys.argv[1:]):
            msg += m
            if idx < len(sys.argv) - 1:
                msg += " "

    pb_key, pvt_key = getKeys()
    encapsulated_text, cipher_text, tag, nonce =  encrypt(msg, pb_key)
    print("Encrypt 1 : ", end="")
    print(encapsulated_text)
    print("cipher text : ")
    print(bytes.hex(cipher_text))
    decode_msg = decrypt(pvt_key, encapsulated_text, cipher_text, tag, nonce)
    print("Decrypted msg...")
    print(decode_msg)
    
    encapsulated_text, cipher_text, tag, nonce =  encrypt(msg, pb_key)
    print("Encrypt 2 : ", end="")
    print(encapsulated_text)
    print("cipher text : ")
    print(bytes.hex(cipher_text))
    decode_msg = decrypt(pvt_key, encapsulated_text, cipher_text, tag, nonce)
    print("Decrypted msg...")
    print(decode_msg)


if __name__ == "__main__":
    main()

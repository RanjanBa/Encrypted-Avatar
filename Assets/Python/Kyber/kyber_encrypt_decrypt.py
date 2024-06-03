import sys
from Crypto.Cipher import AES
from Crypto.Hash import SHA256
from Crypto.Signature import DSS
from Crypto.PublicKey import ECC

from kyber import Kyber1024

# Kyber512.set_drbg_seed(bytes.fromhex('061550234D158C5EC95595FE04EF7A25767F2E24CC2BC479D09D86DC9ABCFDE7056A8C266F9EF97ED08541DBD2E1FFA1'))


def getKey():
    public_key, private_key = Kyber1024.keygen()
    
    return (public_key, private_key)

def encrypt(msg : str, public_key : bytes): 
    cipher_encrypt, shared_key = Kyber1024.enc(public_key)

    # Encrypt the data with the AES session key
    encoded_msg = msg.encode("utf-8")
    cipher_aes = AES.new(shared_key, AES.MODE_EAX)
    ciphertext, tag = cipher_aes.encrypt_and_digest(encoded_msg)

    return (cipher_encrypt, tag, ciphertext, cipher_aes.nonce)
    
def decrypt(private_key : bytes, cipher_encrypt, tag, ciphertext, nonce):
    shared_key = Kyber1024.dec(cipher_encrypt, private_key)
    # Decrypt the data with the AES session key
    cipher_aes = AES.new(shared_key, AES.MODE_EAX, nonce)
    data = cipher_aes.decrypt_and_verify(ciphertext, tag)
    return data.decode("utf-8")


def main():
    msg = "Hello world. I am Ranjan. This is Cryptography test."
    if len(sys.argv) > 1:
        msg = "";
        for idx, m in enumerate(sys.argv[1:]):
            msg += m
            if idx < len(sys.argv) - 1:
                msg += " "

    pb_key, pvt_key = getKey()
    cipher_encrypt, tag, ciphertext, nonce =  encrypt(msg, pb_key)
    # print(cipher_encrypt)
    decode_msg = decrypt(pvt_key, cipher_encrypt, tag, ciphertext, nonce)
    print("cipher text : ")
    print(bytes.hex(ciphertext))
    print("Decrypted msg...")
    print(decode_msg)

if __name__ == "__main__":
    main()
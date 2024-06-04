from Crypto.PublicKey import RSA
from Crypto.Random import get_random_bytes
from Crypto.Cipher import AES, PKCS1_OAEP

def getKey():
    my_key = RSA.generate(2048)
    
    public_key = my_key.publickey().exportKey()
    private_key = my_key.exportKey()
    
    return (public_key, private_key)

def encrypt(msg : str, public_key : bytes):
    encoded_msg = msg.encode("utf-8")
    recipient_key = RSA.import_key(public_key)
    session_key = get_random_bytes(16)

    # Encrypt the session key with the public RSA key
    cipher_rsa = PKCS1_OAEP.new(recipient_key)
    enc_session_key = cipher_rsa.encrypt(session_key)

    # Encrypt the data with the AES session key
    cipher_aes = AES.new(session_key, AES.MODE_EAX)
    ciphertext, tag = cipher_aes.encrypt_and_digest(encoded_msg)
    
    return (enc_session_key, tag, ciphertext, cipher_aes.nonce)
    
def decrypt(private_key : bytes, enc_session_key, tag, ciphertext, nonce):
    private_key = RSA.import_key(private_key)

    # Decrypt the session key with the private RSA key
    cipher_rsa = PKCS1_OAEP.new(private_key)
    session_key = cipher_rsa.decrypt(enc_session_key)

    # Decrypt the data with the AES session key
    cipher_aes = AES.new(session_key, AES.MODE_EAX, nonce)
    data = cipher_aes.decrypt_and_verify(ciphertext, tag)
    return data.decode("utf-8")

def main():
    pb_key, pvt_key = getKey()
    
    enc_session_key, tag, ciphertext, nonce =  encrypt("Hello world. This is Cryptography test.", pb_key)
    decrypt(pvt_key, enc_session_key, tag, ciphertext, nonce)

if __name__ == "__main__":
    main()
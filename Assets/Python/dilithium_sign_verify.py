from Dilithium.dilithium import Dilithium5
from Crypto.Hash import SHA256

def getKeys():
    public_key, private_key = Dilithium5.keygen()
    return (public_key, private_key)

def signature(msg : str, private_key : bytes):
    encoded_msg = msg.encode("utf-8")

    h_object = SHA256.new(encoded_msg)
    h_msg = h_object.digest()

    sign = Dilithium5.sign(private_key, h_msg)
    return sign

def verify(public_key : bytes, sign : bytes, msg : str):
    encoded_msg = msg.encode("utf-8")
    h_object = SHA256.new(encoded_msg)
    h_msg = h_object.digest()

    return Dilithium5.verify(public_key, h_msg, sign)
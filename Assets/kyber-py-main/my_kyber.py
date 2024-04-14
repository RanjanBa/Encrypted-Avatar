from kyber import Kyber512

Kyber512.set_drbg_seed(bytes.fromhex('061550234D158C5EC95595FE04EF7A25767F2E24CC2BC479D09D86DC9ABCFDE7056A8C266F9EF97ED08541DBD2E1FFA1'))
pk, sk = Kyber512.keygen()
public_key = bytes.hex(pk)
secret_key = bytes.hex(sk)

c, sak = Kyber512.enc(pk, 120)
d = Kyber512.dec(c, sk, 120)

# print(bytes.hex(c))
# print()
# print(bytes.hex(sak))
# print()
# print(bytes.hex(d))


print(public_key)
print(secret_key)